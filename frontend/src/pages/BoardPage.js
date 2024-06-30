import {
  Excalidraw,
  exportToSvg,
  serializeAsJSON,
} from '@excalidraw/excalidraw';
import { useCallback, useEffect, useRef, useState } from 'react';
import { useRecoilState } from 'recoil';
import { useBeforeUnload, useNavigate } from 'react-router-dom';
import Select from 'react-select';
import { Loading } from 'react-daisyui';
// eslint-disable-next-line import/no-extraneous-dependencies
import toast from 'react-hot-toast';
// eslint-disable-next-line import/no-extraneous-dependencies
import useWebSocket, { ReadyState } from 'react-use-websocket';
import { Events, WS_ROOM_URL } from '../utils/ws/ws';
import {
  personalPageNeedsUpdate,
  sharedPageNeedsUpdate,
  topBarHeight,
  user,
} from '../state/atoms';
import Modal, { ModalContent } from '../components/Modal';
import { useAuth } from '../hooks/useAuth';
import {
  deleteBoardFromCollection,
  getCollections,
  putBoardToCollection,
} from '../utils/api/collections';
import {
  deleteBoard,
  getBoard,
  postBoard,
  putBoard,
} from '../utils/api/boards';
import Alert from '../components/Alert';
import SaveSVG from '../svg/SaveSVG';
import ArrowLeftSVG from '../svg/ArrowLeftSVG';
import TrashBinWhiteSVG from '../svg/TrashBinWhiteSVG';
import PencilSVG from '../svg/PencilSVG';
import { getAboutId } from '../utils/api/users';

export default function BoardPage(props) {
  const [excalidrawAPI, setExcalidrawAPI] = useState();
  const elements = useRef();
  const appState = useRef();
  const jsonels = useRef();
  const [barHeight] = useRecoilState(topBarHeight);
  const navigate = useNavigate();
  const [saveModalIsOpen, setSaveModalIsOpen] = useState(false);
  const [warningModalIsOpen, setWarningModalIsOpen] = useState(false);
  const [deletedModalIsOpen, setDeletedModalIsOpen] = useState(false);
  const [selectedCollection, setSelectedCollection] = useState(null);
  const [currentBoardCollection, setCurrentBoardCollection] = useState();
  const [allCollections, setAllCollections] = useState();
  const [convertedAllCollections, setConvertedAllCollections] = useState([]);
  const [requestedBoard, setRequestedBoard] = useState();
  const [currentBoard, setCurrentBoard] = useState({
    name: 'My Board',
    id: 0,
    collectionId: 0,
    isShared: false,
    authorId: 0,
  });
  const [currentUser] = useRecoilState(user);
  const [displayAlert, setDisplayAlert] = useState(false);
  const [, setPersonalPageNeedsUpdate] = useRecoilState(
    personalPageNeedsUpdate,
  );
  const [, setSharedPageNeedsUpdate] = useRecoilState(sharedPageNeedsUpdate);
  const [isAutosaving, setIsAutosaving] = useState(false);
  const [intervalId, setIntervalId] = useState(null);
  const [buttonDisabled, setButtonDisabled] = useState(false);
  const [showSaveUpdateLoader, setShowSaveUpdateLoader] = useState(false);
  const [changesMade, setChangesMade] = useState(false);
  const [localBoard, setLocalBoard] = useState(undefined);
  const [showSkeleton, setShowSkeleton] = useState(false);
  const [currentUsersIds, setCurrentUsersIds] = useState([]);
  const [currentUsersNames, setCurrentUsersNames] = useState([]);
  const [currentLeader, setCurrentLeader] = useState('0');
  const { token } = useAuth();
  const { sendMessage, lastMessage, readyState } = useWebSocket(WS_ROOM_URL);
  const [boardUpdateIntervalId, setBoardUpdateIntervalId] = useState(null);

  const exportToSVG = async () => {
    const elsParsed = JSON.parse(jsonels.current);
    const wholeData = {
      elements: elsParsed.elements,
      appState: elsParsed.appState,
    };
    const svg = await exportToSvg({
      elements: wholeData.elements,
      appState: wholeData.appState,
    });
    const s = new XMLSerializer();
    return s.serializeToString(svg);
  };

  const convertAllCollectionsToSelect = () => {
    const selectOptions = [];
    allCollections?.collections?.forEach((collection) => {
      if (collection.role === 'owner' || collection.role === 'moderator') {
        selectOptions.push({
          value: collection.id,
          label: collection.name,
        });
      }
    });
    setConvertedAllCollections(selectOptions);
  };

  const getBoardNameById = (id) => {
    if (allCollections == null) {
      return 'N/A';
    }
    const collection = allCollections?.collections?.find((c) => c.id === id);
    return collection.name;
  };

  const getExcalidraw = async () => {
    elements.current = excalidrawAPI.getSceneElements();
    appState.current = excalidrawAPI.getAppState();
    jsonels.current = serializeAsJSON(elements.current, appState.current);
    const elsParsed = JSON.parse(jsonels.current);
    const picture = await exportToSVG();
    return {
      name: currentBoard.name,
      elements: JSON.stringify(elsParsed.elements),
      appState: JSON.stringify(elsParsed.appState),
      picture,
    };
  };

  const sendNewData = useCallback(() => {
    if (!excalidrawAPI) {
      return;
    }
    getExcalidraw().then((response) => {
      const wholeData = {
        elements: response.elements,
        app_state: response.appState,
      };
      const message = {
        event: Events.NEW_DATA,
        data: wholeData,
        jwt: token,
        board_id: `${currentBoard.id}`,
      };
      sendMessage(JSON.stringify(message));
    });
  });

  const sendConnectRequest = useCallback(() => {
    const url = window.location.href;
    const boardId = url.substring(url.lastIndexOf('/') + 1);
    const message = {
      event: Events.CONNECT,
      jwt: token,
      board_id: `${boardId}`,
    };
    sendMessage(JSON.stringify(message));
  });

  const setLeaderRequest = useCallback(() => {
    const message = {
      event: Events.SET_LEADER,
      jwt: token,
      board_id: `${currentBoard.id}`,
    };
    sendMessage(JSON.stringify(message));
  });

  useBeforeUnload(async (event) => {
    if (!isAutosaving) {
      if (changesMade) {
        event.preventDefault();
      }
    } else {
      const wholeJson = await getExcalidraw();
      await putBoard(currentBoard.id, wholeJson, token);
    }
  });

  const saveBoard = async () => {
    let wholeJson = await getExcalidraw();
    setButtonDisabled(true);
    setShowSaveUpdateLoader(true);
    const response = await postBoard(wholeJson, token);
    wholeJson = {
      collectionId: selectedCollection,
      boardId: response.data.id,
    };
    await putBoardToCollection(wholeJson, token);
    setShowSaveUpdateLoader(false);
    setButtonDisabled(false);
    setSaveModalIsOpen(false);
    setSharedPageNeedsUpdate(true);
    setPersonalPageNeedsUpdate(true);
    navigate('/personal', { replace: true });
  };

  const updateBoard = async (changeCollection) => {
    let wholeJson = await getExcalidraw();
    setLocalBoard(wholeJson);
    setChangesMade(false);
    setButtonDisabled(true);
    setShowSaveUpdateLoader(true);
    await putBoard(currentBoard.id, wholeJson, token);
    if (changeCollection) {
      wholeJson = {
        collectionId: currentBoard.collectionId,
        boardId: currentBoard.id,
      };
      await deleteBoardFromCollection(wholeJson, token);
      wholeJson = {
        collectionId: selectedCollection,
        boardId: currentBoard.id,
      };
      await putBoardToCollection(wholeJson, token);
    }
    setShowSaveUpdateLoader(false);
    setButtonDisabled(false);
    setSaveModalIsOpen(false);
    setSharedPageNeedsUpdate(true);
    setPersonalPageNeedsUpdate(true);
  };

  const removeBoard = async () => {
    setButtonDisabled(true);
    setShowSaveUpdateLoader(true);
    await deleteBoard(currentBoard.id, token);
    setShowSaveUpdateLoader(false);
    setButtonDisabled(false);
    setSharedPageNeedsUpdate(true);
    setPersonalPageNeedsUpdate(true);
    navigate('/personal', { replace: true });
  };

  useEffect(() => {
    if (excalidrawAPI && !props.editMode && !showSkeleton) {
      getExcalidraw().then((response) => {
        setLocalBoard(response);
      });
    }
  }, [excalidrawAPI, showSkeleton]);

  useEffect(() => {
    if (allCollections) {
      convertAllCollectionsToSelect();
      if (props.editMode) {
        const url = window.location.href;
        const boardId = url.substring(url.lastIndexOf('/') + 1);
        getBoard(token, boardId).then((response) => {
          const sceneData = {
            elements: JSON.parse(response.data.elements),
            appState: JSON.parse(response.data.appState),
          };
          setCurrentBoard({
            name: response.data.name,
            id: response.data.id,
            collectionId: response.data.collectionId,
            isShared: response.data.isShared,
            authorId: response.data.authorId,
          });
          setRequestedBoard(sceneData);
          setCurrentBoardCollection(
            allCollections?.collections?.find(
              (c) => c.id === response.data.collectionId,
            ),
          );
          setSelectedCollection(response.data.collectionId);
        });
      } else {
        setShowSkeleton(false);
      }
    }
  }, [allCollections]);

  useEffect(() => {
    if (currentBoardCollection) {
      setShowSkeleton(false);
    }
  }, [currentBoardCollection]);

  useEffect(() => {
    setShowSkeleton(true);
    getCollections(token).then((response) => {
      setAllCollections(response.data);
      if (!props.editMode) {
        setSelectedCollection(response.data.collections[0].id);
      }
    });
  }, []);

  useEffect(() => {
    if (readyState === ReadyState.OPEN && currentBoard?.isShared) {
      sendConnectRequest();
    }
  }, [readyState, currentBoard]);

  useEffect(() => {
    if (requestedBoard && excalidrawAPI) {
      excalidrawAPI.updateScene(requestedBoard);
      excalidrawAPI.scrollToContent();
      getExcalidraw().then((response) => {
        if (!showSkeleton) {
          setLocalBoard(response);
        }
      });
    }
  }, [requestedBoard, excalidrawAPI, showSkeleton]);

  useEffect(() => {
    if (currentUsersIds.length === 0) {
      return;
    }
    setCurrentUsersNames([]);
    currentUsersIds.forEach((u) => {
      getAboutId(u, token).then((response) => {
        setCurrentUsersNames((prev) => {
          return [...prev, response.data.name];
        });
      });
    });
  }, [currentUsersIds]);

  useEffect(() => {
    if (currentLeader === currentUser.name) {
      const interval = setInterval(async () => {
        sendNewData();
      }, 1000);
      setBoardUpdateIntervalId(interval);
    }
    if (currentLeader !== currentUser.name && boardUpdateIntervalId) {
      clearInterval(boardUpdateIntervalId);
    }
  }, [currentLeader]);

  useEffect(() => {
    if (lastMessage == null) {
      return;
    }
    const parsedMessage = JSON.parse(lastMessage?.data);
    switch (parsedMessage.event) {
      case Events.NEW_DATA:
        if (currentUser.name === currentLeader) {
          return;
        }
        // eslint-disable-next-line no-case-declarations
        const parsedData = {
          elements: JSON.parse(parsedMessage.data.elements),
          appState: JSON.parse(parsedMessage.data.app_state),
        };
        excalidrawAPI.updateScene(parsedData);
        break;
      case Events.USER_CONNECTED:
        if (currentUsersIds.length === 0) {
          toast.custom(
            (t) => (
              <div
                className={`${
                  t.visible ? 'intro-animation' : 'outro-animation opacity-0'
                } max-w-md w-full bg-blue-50 border-blue-400 border shadow-lg pointer-events-auto flex ring-1 ring-black ring-opacity-5 rounded-3xl`}
              >
                <div className="flex-1 w-0 p-4">
                  <div className="flex items-start">
                    <div className="ml-3 flex-1">
                      <p className="text-sm font-medium text-gray-900">
                        Successfully connected to the room!
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            ),
            { duration: 1500 },
          );
        }
        // eslint-disable-next-line no-case-declarations
        const newUsers = parsedMessage.user_ids.filter(
          (u) => !currentUsersIds.includes(u),
        );
        setCurrentUsersIds((prev) => [...prev, ...newUsers]);
        if (parsedMessage.leader_id === '0') {
          setCurrentLeader('0');
        } else {
          getAboutId(parsedMessage.leader_id, token).then((response) => {
            setCurrentLeader(response.data.name);
          });
        }
        break;
      case Events.USER_DISCONNECTED:
        for (let i = 0; i < parsedMessage.user_ids.length; i += 1) {
          setCurrentUsersIds((prev) =>
            prev.filter((u) => u === parsedMessage.user_ids[i]),
          );
        }
        if (parsedMessage.leader_id === '0') {
          setCurrentLeader('0');
        } else {
          getAboutId(parsedMessage.leader_id, token).then((response) => {
            setCurrentLeader(response.data.name);
          });
        }
        break;
      case Events.USER_FAILED_TO_CONNECT:
        toast.custom(
          (t) => (
            <div
              className={`${
                t.visible ? 'intro-animation' : ''
              } max-w-md w-full bg-white shadow-lg pointer-events-auto flex ring-1 ring-black ring-opacity-5 rounded-3xl`}
            >
              <div className="flex-1 w-0 p-4">
                <div className="flex items-start">
                  <div className="ml-3 flex-1">
                    <p className="text-sm font-medium text-gray-900">
                      Error: {parsedMessage.reason}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          ),
          { duration: 2000 },
        );
        break;
      case Events.SET_LEADER:
        if (parsedMessage.user_id === '0') {
          setCurrentLeader('0');
        } else {
          getAboutId(parsedMessage.user_id, token).then((response) => {
            setCurrentLeader(response.data.name);
          });
        }
        break;
      default:
        break;
    }
  }, [lastMessage]);

  useEffect(() => {
    if (isAutosaving) {
      getExcalidraw().then((response) => {
        putBoard(currentBoard.id, response, token);
        const interval = setInterval(async () => {
          const wholeJson = await getExcalidraw();
          await putBoard(currentBoard.id, wholeJson, token);
          setLocalBoard(response);
          setChangesMade(false);
        }, 15000);
        setIntervalId(interval);
      });
    } else {
      if (!excalidrawAPI) {
        return;
      }
      if (!intervalId) {
        return;
      }
      clearInterval(intervalId);
      getExcalidraw().then((response) => {
        putBoard(currentBoard.id, response, token);
        setLocalBoard(response);
        setChangesMade(false);
      });
    }
  }, [isAutosaving]);

  const defineBackPage = () => {
    if (!currentBoardCollection) {
      navigate('/personal');
      return;
    }
    if (
      currentBoard.isShared &&
      currentBoardCollection.authorId !== currentUser.id
    ) {
      setSharedPageNeedsUpdate(true);
      navigate('/shared');
    } else if (
      currentBoard.isShared &&
      currentBoardCollection.authorId === currentUser.id
    ) {
      setPersonalPageNeedsUpdate(true);
      navigate('/personal');
    } else if (
      !currentBoard.isShared &&
      currentBoardCollection.authorId === currentUser.id
    ) {
      setPersonalPageNeedsUpdate(true);
      navigate('/personal');
    } else {
      navigate('/personal');
    }
  };

  const h = window.innerHeight;
  const contentHeight = h - barHeight;

  return (
    <div className="flex-auto" style={{ height: contentHeight }}>
      <div className="flex h-full flex-col bg-white">
        {!showSkeleton ? (
          <div className="flex flex-row items-center justify-between h-16 font-normal">
            <div className="flex flex-row space-x-5 items-center">
              <div
                className="border-solid h-8 bg-gray-700 text-white basis-16 ml-3 px-5 py-1 rounded-xl hover:cursor-pointer hover:scale-105 flex justify-center items-center"
                style={{
                  transition: 'all 0.3s ease-in-out',
                  boxShadow: '0px 0px 10px 7px #eee',
                  borderColor: '#eee',
                }}
                onClick={async () => {
                  if (isAutosaving) {
                    clearInterval(intervalId);
                    const wholeJson = await getExcalidraw();
                    await putBoard(currentBoard.id, wholeJson, token);
                    defineBackPage();
                  } else if (changesMade) {
                    setWarningModalIsOpen(true);
                  } else {
                    defineBackPage();
                  }
                }}
              >
                <ArrowLeftSVG width={22} height={22} />
                <div className="ml-2">Back</div>
              </div>
              {props.editMode && currentBoard?.isShared && (
                <div className="rounded-full bg-green-200 w-10 h-10 box-border">
                  <div className="dropdown dropdown-hover h-10 w-10 rounded-full">
                    <div
                      tabIndex={0}
                      role="button"
                      className="h-10 w-10 rounded-full text-center flex items-center justify-center"
                    >
                      <div className="font-bold">{currentUsersIds.length}</div>
                    </div>
                    <div className="dropdown-content z-50 menu shadow bg-gray-50 rounded-box w-36">
                      {currentUsersNames?.map((u) => (
                        <div
                          key={u}
                          className="flex flex-row items-center my-2 mx-2 w-28"
                        >
                          <div className="avatar placeholder">
                            <div className="bg-neutral text-neutral-content rounded-full w-7">
                              <span className="text-xs">{u[0] + u[1]}</span>
                            </div>
                          </div>
                          <div className="ml-2 truncate">{u}</div>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              )}
              {props.editMode &&
                currentBoard?.isShared &&
                currentBoardCollection?.role !== 'reader' && (
                  <div
                    className={`border hover:cursor-pointer rounded-3xl flex flex-row justify-center items-center space-x-3 py-1 px-2 text-indigo-700 bg-indigo-100 border-indigo-800 ${currentLeader === currentUser.name || currentLeader === '0' ? '' : 'hidden disabled'}`}
                    onClick={() => {
                      setLeaderRequest();
                    }}
                  >
                    <div className="text-sm">
                      {currentLeader === currentUser.name &&
                      currentBoard?.isShared
                        ? 'Drop Leader'
                        : 'Take Leader'}
                    </div>
                    <PencilSVG
                      width={22}
                      height={22}
                      color="stroke-indigo-700"
                    />
                  </div>
                )}
              {props.editMode &&
                currentBoard?.isShared &&
                currentBoardCollection?.role !== 'reader' && (
                  <div className="border hover:cursor-pointer rounded-3xl flex flex-row justify-center items-center space-x-2 py-1 px-2 text-indigo-700 bg-indigo-100 border-indigo-800">
                    <div className="text-sm">Current Leader:</div>
                    <div className="text-sm">
                      {currentLeader === '0' ? 'None' : currentLeader}
                    </div>
                  </div>
                )}
            </div>
            {currentBoardCollection?.role !== 'reader' && (
              <div
                className={`px-2 ml-auto mr-5 p-0.5 h-7 text-xs border border-orange-300 font-medium tracking-wide text-orange-500 bg-orange-50 rounded-full text-center flex flex-row items-center justify-center transition-opacity duration-500 ${
                  changesMade && !isAutosaving ? 'opacity-100' : 'opacity-0'
                } ${(currentLeader === currentUser.name && currentBoard?.isShared) || !currentBoard.isShared || !props.editMode ? '' : 'hidden disabled'}`}
              >
                Unsaved changes ⚠️
              </div>
            )}
            {props.editMode && currentBoardCollection?.role !== 'reader' && (
              <div
                className={`px-2 py-1 h-8 my-4 ml-4 text-xs font-medium hover:cursor-pointer transition duration-500 border tracking-wide rounded-full text-center flex flex-row items-center justify-center${
                  isAutosaving
                    ? ' hover:border-green-500 border-green-300 text-green-600 bg-green-50'
                    : ' hover:border-gray-500 border-gray-300 text-gray-600 bg-gray-100'
                } ${(currentLeader === currentUser.name && currentBoard?.isShared) || !currentBoard.isShared || !props.editMode ? '' : 'hidden disabled'}`}
                onClick={() => {
                  setIsAutosaving(!isAutosaving);
                }}
              >
                <div>Autosave:</div>
                <div className="ml-2">{isAutosaving ? 'ON' : 'OFF'}</div>
                {isAutosaving && (
                  <div className="pt-1 ml-1">
                    <Loading variant="ring" size="sm" />
                  </div>
                )}
              </div>
            )}
            <div className="flex flex-row justify-center items-center p-4 gap-x-2">
              {currentBoardCollection?.role !== 'reader' && (
                <div
                  className={`flex justify-center items-center hover:cursor-pointer ${(currentLeader === currentUser.name && currentBoard?.isShared) || !currentBoard.isShared || !props.editMode ? '' : 'hidden disabled'}`}
                >
                  <div
                    className="border-solid bg-blue-700 text-white w-32 px-5 py-1 rounded-xl hover:cursor-pointer hover:scale-105 flex justify-center items-center"
                    onClick={() => {
                      setSaveModalIsOpen(true);
                    }}
                    style={{
                      transition: 'all 0.3s ease-in-out',
                      boxShadow: '0px 0px 10px 7px #eee',
                      borderColor: '#eee',
                    }}
                  >
                    <SaveSVG width={20} height={20} />
                    <div className="ml-2">
                      {props?.editMode ? 'Save as' : 'Save'}
                    </div>
                  </div>
                </div>
              )}
              {(currentBoardCollection?.role === 'owner' ||
                currentBoardCollection?.role === 'moderator') && (
                <div
                  className={
                    (props.editMode
                      ? 'flex-shrink border-solid bg-red-600 w-32 text-white basis-16 ml-3 px-5 py-1 rounded-xl hover:cursor-pointer hover:scale-105 flex justify-center items-center '
                      : 'hidden ') +
                    ((currentLeader === currentUser.name &&
                      currentBoard?.isShared) ||
                    !currentBoard.isShared ||
                    !props.editMode
                      ? ''
                      : 'hidden disabled')
                  }
                  onClick={async () => {
                    setDeletedModalIsOpen(true);
                  }}
                  style={{
                    transition: 'all 0.3s ease-in-out',
                    boxShadow: '0px 0px 10px 7px #eee',
                    borderColor: '#eee',
                  }}
                >
                  <TrashBinWhiteSVG width={20} height={20} />
                  <div className="ml-2">Delete</div>
                </div>
              )}
            </div>
          </div>
        ) : (
          <div className="skeleton my-4 h-8 mx-3" />
        )}
        {!showSkeleton ? (
          <div className="flex-grow mx-3 mb-3 border-2 rounded-l">
            <Excalidraw
              excalidrawAPI={(api) => setExcalidrawAPI(api)}
              onChange={async () => {
                if (!localBoard) {
                  return;
                }
                if (
                  (!changesMade &&
                    currentBoard.isShared &&
                    currentLeader === currentUser.name) ||
                  (!changesMade && !currentBoard.isShared && props.editMode) ||
                  (!changesMade && !props.editMode)
                ) {
                  const newData = await getExcalidraw();
                  if (newData.elements !== localBoard.elements) {
                    setChangesMade(true);
                  }
                }
              }}
              viewModeEnabled={
                currentBoardCollection?.role === 'reader' ||
                (currentBoard.isShared && currentLeader !== currentUser.name)
              }
            />
          </div>
        ) : (
          <div className="flex-grow rounded-l mx-3 mb-3 flex">
            <Excalidraw excalidrawAPI={(api) => setExcalidrawAPI(api)} />
            <div
              className="skeleton w-full h-full z-50"
              style={{
                marginLeft: '-100%',
              }}
            />
          </div>
        )}
      </div>
      <Modal
        isOpen={saveModalIsOpen}
        handleClose={() => {
          setSaveModalIsOpen(false);
          setDisplayAlert(false);
        }}
      >
        <ModalContent>
          <div className="flex flex-col justify-center items-center p-14">
            {(currentBoardCollection?.role === 'owner' ||
              currentBoardCollection?.role === 'moderator') && (
              <div className="flex flex-row justify-between space-x-7 items-center mb-5">
                <div className="font-light italic text-black w-40">
                  Board name:
                </div>
                <input
                  className="block w-56 rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 flex-shrink"
                  placeholder="My Board"
                  value={currentBoard.name}
                  onChange={(event) => {
                    setCurrentBoard({
                      name: event.target.value,
                      id: currentBoard.id,
                      collectionId: currentBoard.collectionId,
                      isShared: currentBoard.isShared,
                    });
                  }}
                />
              </div>
            )}
            {((currentBoardCollection?.role === 'owner' &&
              currentBoard.authorId === currentUser.id) ||
              !props?.editMode) && (
              <div className="flex flex-row justify-between space-x-7 items-center mb-5">
                <div className="font-light italic text-black w-40">
                  Add this board to:
                </div>
                <div className="w-56">
                  <Select
                    options={convertedAllCollections}
                    placeholder={
                      selectedCollection
                        ? getBoardNameById(selectedCollection)
                        : ''
                    }
                    defaultValue={selectedCollection}
                    onChange={(newValue) => {
                      setSelectedCollection(newValue.value);
                    }}
                  />
                </div>
              </div>
            )}
            <div
              className={`mt-7 w-full mb-4 transition-opacity duration-300 h-8${displayAlert ? ' opacity-100' : ' opacity-0'}`}
            >
              <Alert
                title="Error!"
                padding="p-3"
                description="No collection was specified, choose one"
              />
            </div>
            <button
              type="submit"
              className={`flex w-full h-8 justify-center items-center rounded-md bg-indigo-600 px-3 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600${
                buttonDisabled ? ' cursor-not-allowed disabled' : ''
              }`}
              onClick={async () => {
                if (selectedCollection == null) {
                  setDisplayAlert(true);
                  return;
                }
                if (buttonDisabled) {
                  return;
                }
                setDisplayAlert(false);
                if (props.editMode) {
                  if (currentBoardCollection?.role === 'owner') {
                    if (currentBoard.authorId === currentUser.id) {
                      await updateBoard(true);
                    } else {
                      await updateBoard(false);
                    }
                  } else {
                    await updateBoard(false);
                  }
                } else {
                  await saveBoard();
                }
              }}
            >
              {props.editMode ? (
                <div>
                  {showSaveUpdateLoader ? (
                    <div
                      className={`pt-1.5 transition${
                        showSaveUpdateLoader ? 'opacity-100' : 'opacity-0'
                      }`}
                    >
                      <Loading variant="dots" size="sm" />
                    </div>
                  ) : (
                    <div className="py-0.5">Update</div>
                  )}
                </div>
              ) : (
                <div>
                  {showSaveUpdateLoader ? (
                    <div
                      className={`pt-1.5 transition${
                        showSaveUpdateLoader ? 'opacity-100' : 'opacity-0'
                      }`}
                    >
                      <Loading variant="dots" size="sm" />
                    </div>
                  ) : (
                    <div className="py-0.5">Create</div>
                  )}
                </div>
              )}
            </button>
          </div>
        </ModalContent>
      </Modal>
      <Modal
        isOpen={warningModalIsOpen}
        handleClose={() => {
          setWarningModalIsOpen(false);
        }}
      >
        <ModalContent>
          <div className="flex flex-col justify-center items-center p-14">
            <div className="flex flex-col justify-between items-center">
              <div className="font-normal italic text-black w-full mx-4 text-center">
                Your changes will be lost, are you sure?
              </div>
              <div className="flex flex-row items-center justify-center mt-8 w-full">
                <button
                  type="submit"
                  className="flex w-full justify-center transition duration-300 mx-4 rounded-3xl bg-red-500 px-3 py-1 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-red-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                  onClick={async () => {
                    navigate('/personal');
                  }}
                >
                  Yes
                </button>
                <button
                  type="submit"
                  className="flex w-full justify-center transition duration-300 mx-4 rounded-3xl bg-blue-500 px-3 py-1 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-blue-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-red-600"
                  onClick={async () => {
                    setWarningModalIsOpen(false);
                  }}
                >
                  No
                </button>
              </div>
            </div>
          </div>
        </ModalContent>
      </Modal>
      <Modal
        isOpen={deletedModalIsOpen}
        handleClose={() => {
          setDeletedModalIsOpen(false);
        }}
      >
        <ModalContent>
          <div className="flex flex-col justify-center items-center p-14">
            <div className="flex flex-col justify-between items-center">
              <div className="font-normal italic text-black w-full mx-4 text-center">
                Are you sure you want to delete this board?
              </div>
              <div className="flex flex-row items-center justify-center mt-8 w-full">
                <button
                  type="submit"
                  className={`flex w-full justify-center items-center h-8 transition duration-300 mx-4 rounded-3xl bg-red-500 px-3 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-red-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600${
                    buttonDisabled ? ' cursor-not-allowed disabled' : ''
                  }`}
                  onClick={async () => {
                    if (buttonDisabled) {
                      return;
                    }
                    await removeBoard();
                  }}
                >
                  {showSaveUpdateLoader ? (
                    <div
                      className={`mt-1.5 transition${
                        showSaveUpdateLoader ? 'opacity-100' : 'opacity-0'
                      }`}
                    >
                      <Loading variant="dots" size="sm" />
                    </div>
                  ) : (
                    <div className="">Yes</div>
                  )}
                </button>
                <button
                  type="submit"
                  className={`flex w-full justify-center transition duration-300 mx-4 rounded-3xl bg-blue-500 px-3 py-1 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-blue-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-red-600${
                    buttonDisabled ? ' cursor-not-allowed disabled' : ''
                  }`}
                  onClick={async () => {
                    setDeletedModalIsOpen(false);
                  }}
                >
                  No
                </button>
              </div>
            </div>
          </div>
        </ModalContent>
      </Modal>
    </div>
  );
}
