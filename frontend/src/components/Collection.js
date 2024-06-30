import { useRecoilState } from 'recoil';
import { useNavigate } from 'react-router-dom';
import { useEffect, useRef, useState } from 'react';
import autoAnimate from '@formkit/auto-animate';
import { Checkbox, Divider, Loading, Tooltip } from 'react-daisyui';
import BoardCard from './BoardCard';
import { personalPageNeedsUpdate, user } from '../state/atoms';
import ArrowUpSVG from '../svg/ArrowUpSVG';
import PencilSVG from '../svg/PencilSVG';
import Modal, { ModalContent } from './Modal';
import {
  deleteCollection,
  deleteSharedRoles,
  getSharedRoles,
  putCollection,
  putSharedRoles,
} from '../utils/api/collections';
import { useAuth } from '../hooks/useAuth';
import TrashBinSVG from '../svg/TrashBinSVG';
import LockSVG from '../svg/LockSVG';
import PeopleSVG from '../svg/PeopleSVG';
import ShareSVG from '../svg/ShareSVG';
import InfoSVG from '../svg/InfoSVG';
import PlusSVG from '../svg/PlusSVG';
import { getAboutId, getAboutName } from '../utils/api/users';
import Alert from './Alert';

export default function Collection(props) {
  const [hiddenIds, setHiddenIds] = useState([]);
  const [editModalIsOpen, setEditModalIsOpen] = useState(false);
  const [currentCollection, setCurrentCollection] = useState({
    name: props.name,
    id: props.id,
  });
  const [, setNeedsUpdate] = useRecoilState(personalPageNeedsUpdate);
  const [currentUser] = useRecoilState(user);
  const navigate = useNavigate();
  const { token } = useAuth();
  const parentRef = useRef();
  const [shareModalIsOpen, setShareModalIsOpen] = useState(false);
  const [deleteModalIsOpen, setDeleteModalIsOpen] = useState(false);
  const [buttonDisabled, setButtonDisabled] = useState(false);
  const [showSaveUpdateLoader, setShowSaveUpdateLoader] = useState(false);
  const [previousReaders, setPreviousReaders] = useState([]);
  const [previousEditors, setPreviousEditors] = useState([]);
  const [previousModerators, setPreviousModerators] = useState([]);
  const [currentReaders, setCurrentReaders] = useState([]);
  const [currentEditors, setCurrentEditors] = useState([]);
  const [currentModerators, setCurrentModerators] = useState([]);
  const [currentAnonym, setCurrentAnonym] = useState(false);
  const [currentReaderInput, setCurrentReaderInput] = useState('');
  const [currentEditorInput, setCurrentEditorInput] = useState('');
  const [currentModeratorInput, setCurrentModeratorInput] = useState('');
  const [authorName, setAuthorName] = useState('•');
  const [showReaderInputErrorTooltip, setShowReaderInputErrorTooltip] =
    useState(false);
  const [showEditorInputErrorTooltip, setShowEditorInputErrorTooltip] =
    useState(false);
  const [showModeratorInputErrorTooltip, setShowModeratorInputErrorTooltip] =
    useState(false);

  useEffect(() => {
    if (parentRef.current) {
      autoAnimate(parentRef.current);
    }
  }, [parentRef]);

  const select = (collectionId) => {
    if (hiddenIds.indexOf(collectionId) >= 0) {
      setHiddenIds(
        hiddenIds.filter((x) => {
          return x !== collectionId;
        }),
      );
    } else {
      setHiddenIds([...hiddenIds, collectionId]);
    }
  };

  const checkUsernameExists = async (username) => {
    const response = await getAboutName(username, token);
    return response.status === 200;
  };

  const tryAddUserToReaders = async (username) => {
    if (
      (await checkUsernameExists(username)) &&
      currentUser.name !== username
    ) {
      setCurrentReaders([...currentReaders, username]);
      return;
    }
    setShowReaderInputErrorTooltip(true);
    const timeout = setTimeout(() => {
      setShowReaderInputErrorTooltip(false);
      clearTimeout(timeout);
    }, 1500);
  };

  const tryAddUserToEditors = async (username) => {
    if (
      (await checkUsernameExists(username)) &&
      currentUser.name !== username
    ) {
      setCurrentEditors([...currentEditors, username]);
      return;
    }
    setShowEditorInputErrorTooltip(true);
    const timeout = setTimeout(() => {
      setShowEditorInputErrorTooltip(false);
      clearTimeout(timeout);
    }, 1500);
  };

  const tryAddUserToModerators = async (username) => {
    if (
      (await checkUsernameExists(username)) &&
      currentUser.name !== username
    ) {
      setCurrentModerators([...currentModerators, username]);
      return;
    }
    setShowModeratorInputErrorTooltip(true);
    const timeout = setTimeout(() => {
      setShowModeratorInputErrorTooltip(false);
      clearTimeout(timeout);
    }, 1500);
  };

  const removeCollection = async () => {
    setShowSaveUpdateLoader(true);
    setButtonDisabled(true);
    await deleteCollection(props.id, token);
    setDeleteModalIsOpen(false);
    setShowSaveUpdateLoader(false);
    setButtonDisabled(false);
    setNeedsUpdate(true);
  };

  const updateCollection = async () => {
    const wholeJson = {
      name: currentCollection.name,
    };
    await putCollection(props.id, wholeJson, token);
    setNeedsUpdate(true);
    setEditModalIsOpen(false);
  };

  const diffRoles = (newState, previousState) => {
    const output = {};
    output.readersToAdd = newState.reader.filter(
      (x) => !previousState.reader.includes(x),
    );
    output.readersToRemove = previousState.reader.filter(
      (x) => !newState.reader.includes(x),
    );
    output.editorsToAdd = newState.editor.filter(
      (x) => !previousState.editor.includes(x),
    );
    output.editorsToRemove = previousState.editor.filter(
      (x) => !newState.editor.includes(x),
    );
    output.moderatorsToAdd = newState.moderator.filter(
      (x) => !previousState.moderator.includes(x),
    );
    output.moderatorsToRemove = previousState.moderator.filter(
      (x) => !newState.moderator.includes(x),
    );
    return output;
  };

  const updateSharingSettings = async () => {
    const currentRoles = {
      reader: currentReaders,
      editor: currentEditors,
      moderator: currentModerators,
    };
    const previousRoles = {
      reader: previousReaders,
      editor: previousEditors,
      moderator: previousModerators,
    };
    const diff = diffRoles(currentRoles, previousRoles);
    const rolesToAssign = {
      reader: diff.readersToAdd,
      editor: diff.editorsToAdd,
      moderator: diff.moderatorsToAdd,
      anonym: currentAnonym,
    };
    const rolesToRemove = {
      reader: diff.readersToRemove,
      editor: diff.editorsToRemove,
      moderator: diff.moderatorsToRemove,
    };
    await putSharedRoles(props.id, rolesToAssign, token);
    await deleteSharedRoles(props.id, rolesToRemove, token);
    setNeedsUpdate(true);
    setShareModalIsOpen(false);
  };

  const getSharingSettings = async () => {
    return getSharedRoles(props.id, token);
  };

  useEffect(() => {
    if (shareModalIsOpen) {
      getSharingSettings().then((response) => {
        setCurrentReaders(response.data.reader);
        setCurrentEditors(response.data.editor);
        setCurrentModerators(response.data.moderator);
        setCurrentAnonym(response.data.anonym);

        setPreviousReaders(response.data.reader);
        setPreviousEditors(response.data.editor);
        setPreviousModerators(response.data.moderator);
      });
    }
  }, [shareModalIsOpen]);

  useEffect(() => {
    if (props.author !== currentUser.id && authorName === '•') {
      getAboutId(props.author, token).then((response) => {
        setAuthorName(response.data.name);
      });
    }
  }, []);

  return (
    <li
      className="max-w-full rounded-3xl bg-white flex flex-col w-full overflow-hidden"
      style={{
        boxShadow: '0px 0px 20px 7px #eee',
        borderColor: '#dcdcdc',
      }}
    >
      <div ref={parentRef}>
        <div className="flex flex-row justify-between items-center m-3 space-x-2">
          <div className="text-xl tracking-tight text-black flex flex-row flex-grow items-center">
            <h5 className="min-w-40">{props?.name}</h5>
            {props?.isShared && props?.displaySharedLabel && (
              <div className="px-2 ml-5 p-0.5 h-5 text-xs font-medium tracking-wide text-blue-500 bg-blue-50 rounded-full text-center flex flex-row">
                Public
                <div className="ml-2">
                  <PeopleSVG width={15} heigth={15} stroke="blue" />
                </div>
              </div>
            )}
            {!props?.isShared && props?.displaySharedLabel && (
              <div className="px-2 ml-5 p-0.5 h-5 text-xs font-medium tracking-wide text-red-500 bg-red-50 rounded-full text-center flex flex-row">
                Private
                <div className="ml-2">
                  <LockSVG width={15} heigth={15} stroke="red" />
                </div>
              </div>
            )}
            {props?.author && props?.displayAuthorLabel && (
              <div className="px-2 ml-5 p-0.5 h-5 text-xs font-medium tracking-wide text-indigo-500 bg-indigo-50 rounded-full text-center flex flex-row">
                {`From: ${authorName}`}
              </div>
            )}
            {props?.author &&
              props?.displayAuthorLabel &&
              props?.role === 'moderator' && (
                <div className="px-2 ml-5 p-0.5 h-5 text-xs font-medium tracking-wide text-green-500 bg-green-50 rounded-full text-center flex flex-row">
                  Moderator
                </div>
              )}
            {props?.author &&
              props?.displayAuthorLabel &&
              props?.role === 'editor' && (
                <div className="px-2 ml-5 p-0.5 h-5 text-xs font-medium tracking-wide text-blue-500 bg-blue-50 rounded-full text-center flex flex-row">
                  Editor
                </div>
              )}
            {props?.author &&
              props?.displayAuthorLabel &&
              props?.role === 'reader' && (
                <div className="px-2 ml-5 p-0.5 h-5 text-xs font-medium tracking-wide text-red-500 bg-red-50 rounded-full text-center flex flex-row">
                  Reader
                </div>
              )}
          </div>
          {props.author === currentUser.id && (
            <div
              onClick={() => {
                setShareModalIsOpen(true);
              }}
              style={{
                transition: 'transform 0.3s ease-in-out',
              }}
              className="hover:scale-110 hover:cursor-pointer"
            >
              <ShareSVG width={32} heigth={32} animate />
            </div>
          )}
          {props.author === currentUser.id && (
            <div
              onClick={() => {
                setEditModalIsOpen(true);
              }}
              style={{
                transition: 'transform 0.3s ease-in-out',
              }}
              className="hover:scale-110 hover:cursor-pointer"
            >
              <PencilSVG width={32} heigth={32} animate />
            </div>
          )}
          {props.author === currentUser.id && (
            <div
              onClick={() => {
                setDeleteModalIsOpen(true);
              }}
              style={{
                transition: 'transform 0.3s ease-in-out',
              }}
              className="hover:scale-110 hover:cursor-pointer"
            >
              <TrashBinSVG width={32} heigth={32} animate />
            </div>
          )}
          <div
            className={
              hiddenIds.indexOf(props.id) !== -1
                ? 'transition-transform duration-300 flex flex-row justify-center items-center hover:scale-110 hover:cursor-pointer'
                : 'rotate-180 transition-transform duration-300 flex flex-row justify-center items-center hover:scale-110 hover:cursor-pointer'
            }
            onClick={() => {
              select(props.id);
            }}
          >
            <ArrowUpSVG width={32} heigth={32} />
          </div>
        </div>
        {hiddenIds.indexOf(props.id) !== -1 && (
          <div className="flex flex-wrap flex-row p-4 justify-evenly box-border gap-5 mt-6 mb-6">
            {props?.boards.length === 0 ? (
              <div className="font-extralight italic text-black text-center">
                No any boards
              </div>
            ) : (
              props?.boards?.map((board) => (
                <BoardCard
                  key={board.id}
                  name={board.name}
                  picture={board.picture}
                  appState={board.appState}
                  select={() => {
                    navigate(`/board/${board.id}`, { replace: true });
                  }}
                />
              ))
            )}
          </div>
        )}
      </div>
      <Modal
        isOpen={editModalIsOpen}
        handleClose={() => {
          setEditModalIsOpen(false);
        }}
      >
        <ModalContent>
          <div className="flex flex-col justify-center items-center p-14">
            <div className="flex flex-row justify-between space-x-7 items-center mb-5">
              <div className="font-light italic text-black w-40">
                Collection name:
              </div>
              <input
                className="block w-56 rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 flex-shrink"
                placeholder="My Collection"
                value={currentCollection.name}
                onChange={(event) => {
                  setCurrentCollection({
                    name: event.target.value,
                    id: currentCollection.id,
                  });
                }}
              />
            </div>
            <button
              type="submit"
              className="flex w-full justify-center rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
              onClick={async () => {
                await updateCollection();
              }}
            >
              Update
            </button>
          </div>
        </ModalContent>
      </Modal>
      <Modal
        isOpen={shareModalIsOpen}
        handleClose={() => {
          setShareModalIsOpen(false);
        }}
      >
        <ModalContent>
          <div className="flex flex-col w-full justify-center items-center p-14">
            <div className="flex flex-col w-full justify-between items-center mb-5">
              <div className="font-bold text-lg text-center mb-2 flex-row flex items-center justify-center space-x-5">
                <div>Sharing settings</div>
                <span className="px-2 h-5 text-xs font-medium tracking-wide text-green-600 border border-green-600 rounded-full text-center flex justify-center items-center">
                  Beta
                </span>
              </div>
              <div className="text-gray-400 text-sm text-center mb-3">
                Hint: to add a user, type their username. To remove, click on
                their badge.
              </div>
              <ul className="flex flex-row justify-between items-center space-x-6">
                <li className="w-52">
                  <div
                    className={`mx-4 mb-4 transition-opacity duration-300 h-8${showModeratorInputErrorTooltip ? ' opacity-100' : ' opacity-0'}`}
                  >
                    <Alert
                      title=""
                      padding="p-1"
                      description="User not found."
                    />
                  </div>
                  <div className="border border-green-200 rounded-lg">
                    <div className="inline-flex items-center justify-center w-full p-3 text-gray-500 bg-white rounded-lg space-x-5">
                      <div className="block">
                        <div className="w-full text-sm text-green-600">
                          Moderators
                        </div>
                      </div>
                      <Tooltip
                        message="User(s) will be able to VIEW, EDIT, and DELETE boards in this collection."
                        position="top"
                      >
                        <InfoSVG width={17} heigth={17} />
                      </Tooltip>
                    </div>
                    <div className="flex flex-row justify-between items-center">
                      <input
                        className="block w-32 ml-2 h-7 rounded-md border text-gray-700 text-sm p-1.5"
                        value={currentModeratorInput}
                        onChange={(event) => {
                          setCurrentModeratorInput(event.target.value);
                        }}
                      />
                      <div
                        className="mx-auto hover:cursor-pointer hover:scale-110 transition duration-500"
                        onClick={async () => {
                          if (
                            currentModeratorInput &&
                            currentModeratorInput.length > 0
                          ) {
                            if (
                              currentEditors?.includes(currentModeratorInput)
                            ) {
                              setCurrentEditors(
                                currentEditors.filter(
                                  (x) => x !== currentModeratorInput,
                                ),
                              );
                            }
                            if (
                              currentReaders?.includes(currentModeratorInput)
                            ) {
                              setCurrentReaders(
                                currentReaders.filter(
                                  (x) => x !== currentModeratorInput,
                                ),
                              );
                            }
                            if (
                              currentModerators?.includes(currentModeratorInput)
                            ) {
                              setCurrentModeratorInput('');
                              return;
                            }
                            await tryAddUserToModerators(currentModeratorInput);
                            setCurrentModeratorInput('');
                          }
                        }}
                      >
                        <PlusSVG
                          width={20}
                          heigth={20}
                          color="fill-green-600"
                        />
                      </div>
                    </div>
                    <Divider>
                      <div className="text-sm font-extralight text-gray-500">
                        List
                      </div>
                    </Divider>
                    <div className="h-36 overflow-y-scroll flex flex-wrap p-1">
                      {currentModerators?.map((moderator) => (
                        <div
                          key={moderator}
                          className="px-2 ml-1 p-0.5 h-5 text-xs font-medium tracking-wide text-green-500 bg-green-50 rounded-full text-center flex flex-row justify-center items-center hover:scale-101 hover:border-green-400 hover:border hover:cursor-pointer transition duration-500"
                          onClick={() => {
                            setCurrentModerators(
                              currentModerators.filter((x) => x !== moderator),
                            );
                          }}
                        >
                          {moderator}
                        </div>
                      ))}
                    </div>
                  </div>
                </li>
                <li className="w-52">
                  <div
                    className={`mx-4 mb-4 transition-opacity duration-300 h-8${showEditorInputErrorTooltip ? ' opacity-100' : ' opacity-0'}`}
                  >
                    <Alert
                      title=""
                      padding="p-1"
                      description="User not found."
                    />
                  </div>
                  <div className="border border-blue-200 rounded-lg">
                    <div className="inline-flex items-center justify-center w-full p-3 text-gray-500 bg-white rounded-lg space-x-5">
                      <div className="block">
                        <div className="w-full text-sm text-blue-600">
                          Editors
                        </div>
                      </div>
                      <Tooltip
                        message="User(s) will be able to VIEW and EDIT boards in this collection."
                        position="top"
                      >
                        <InfoSVG width={17} heigth={17} />
                      </Tooltip>
                    </div>
                    <div className="flex flex-row justify-between items-center">
                      <input
                        className="block w-32 ml-2 h-7 rounded-md border text-gray-700 text-sm p-1.5"
                        value={currentEditorInput}
                        onChange={(event) => {
                          setCurrentEditorInput(event.target.value);
                        }}
                      />
                      <div
                        className="mx-auto hover:cursor-pointer hover:scale-110 transition duration-500"
                        onClick={async () => {
                          if (
                            currentEditorInput &&
                            currentEditorInput.length > 0
                          ) {
                            if (currentReaders?.includes(currentEditorInput)) {
                              setCurrentReaders(
                                currentReaders.filter(
                                  (x) => x !== currentEditorInput,
                                ),
                              );
                            }
                            if (
                              currentModerators?.includes(currentEditorInput)
                            ) {
                              setCurrentModerators(
                                currentModerators.filter(
                                  (x) => x !== currentEditorInput,
                                ),
                              );
                            }
                            if (currentEditors?.includes(currentEditorInput)) {
                              setCurrentEditorInput('');
                              return;
                            }
                            await tryAddUserToEditors(currentEditorInput);
                            setCurrentEditorInput('');
                          }
                        }}
                      >
                        <PlusSVG width={20} heigth={20} color="fill-blue-600" />
                      </div>
                    </div>
                    <Divider>
                      <div className="text-sm font-extralight text-gray-500">
                        List
                      </div>
                    </Divider>
                    <div className="h-36 overflow-y-scroll flex flex-wrap p-1">
                      {currentEditors?.map((editor) => (
                        <div
                          key={editor}
                          className="px-2 ml-1 p-0.5 h-5 text-xs font-medium tracking-wide text-blue-500 bg-blue-50 rounded-full text-center flex flex-row justify-center items-center hover:scale-101 hover:border-blue-400 hover:border hover:cursor-pointer transition duration-500"
                          onClick={() => {
                            setCurrentEditors(
                              currentEditors.filter((x) => x !== editor),
                            );
                          }}
                        >
                          {editor}
                        </div>
                      ))}
                    </div>
                  </div>
                </li>
                <li className="w-52">
                  <div
                    className={`mx-4 mb-4 transition-opacity duration-300 h-8${showReaderInputErrorTooltip ? ' opacity-100' : ' opacity-0'}`}
                  >
                    <Alert
                      title=""
                      padding="p-1"
                      description="User not found."
                    />
                  </div>
                  <div className="border border-red-200 rounded-lg">
                    <div className="inline-flex items-center justify-center w-full p-3 text-gray-500 bg-white rounded-lg space-x-5">
                      <div className="block">
                        <div className="w-full text-sm text-red-600">
                          Readers
                        </div>
                      </div>
                      <Tooltip
                        message="User(s) will be able to VIEW boards in this collection."
                        position="top"
                      >
                        <InfoSVG width={17} heigth={17} />
                      </Tooltip>
                    </div>
                    <div className="flex flex-row justify-between items-center">
                      <input
                        className="block w-32 ml-2 h-7 rounded-md border text-gray-700 text-sm p-1.5"
                        value={currentReaderInput}
                        onChange={(event) => {
                          setCurrentReaderInput(event.target.value);
                        }}
                      />
                      <div
                        className="mx-auto hover:cursor-pointer hover:scale-110 transition duration-500"
                        onClick={async () => {
                          if (
                            currentReaderInput &&
                            currentReaderInput.length > 0
                          ) {
                            if (currentEditors?.includes(currentReaderInput)) {
                              setCurrentEditors(
                                currentEditors.filter(
                                  (x) => x !== currentReaderInput,
                                ),
                              );
                            }
                            if (
                              currentModerators?.includes(currentReaderInput)
                            ) {
                              setCurrentModerators(
                                currentModerators?.filter(
                                  (x) => x !== currentReaderInput,
                                ),
                              );
                            }
                            if (currentReaders?.includes(currentReaderInput)) {
                              setCurrentReaderInput('');
                              return;
                            }
                            await tryAddUserToReaders(currentReaderInput);
                            setCurrentReaderInput('');
                          }
                        }}
                      >
                        <PlusSVG width={20} heigth={20} color="fill-red-600" />
                      </div>
                    </div>
                    <Divider>
                      <div className="text-sm font-extralight text-gray-500">
                        List
                      </div>
                    </Divider>
                    <div className="h-36 overflow-y-scroll flex flex-wrap p-1">
                      {currentReaders?.map((reader) => (
                        <div
                          key={reader}
                          className="px-2 ml-1 p-0.5 h-5 text-xs font-medium tracking-wide text-red-500 bg-red-50 rounded-full text-center flex flex-row justify-center items-center hover:scale-101 hover:border-red-400 hover:border hover:cursor-pointer transition duration-500"
                          onClick={() => {
                            setCurrentReaders(
                              currentReaders.filter((x) => x !== reader),
                            );
                          }}
                        >
                          {reader}
                        </div>
                      ))}
                    </div>
                  </div>
                </li>
              </ul>
              <div className="flex flex-row items-center my-7 w-full h-4">
                <div className="flex flex-row items-center">
                  <Checkbox
                    className="h-5 w-5"
                    color="primary"
                    checked={currentAnonym}
                    onChange={() => {
                      setCurrentAnonym(!currentAnonym);
                    }}
                  />
                  <div className="text-sm text-gray-500 mx-2">
                    Allow anonymous users
                  </div>
                  <Tooltip
                    message="Anyone with the link will be able to VIEW boards in this collection."
                    position="top"
                  >
                    <InfoSVG width={17} heigth={17} />
                  </Tooltip>
                  {currentAnonym && (
                    <div className="border rounded-3xl border-blue-600 flex flex-row items-center ml-10">
                      <div className="py-1.5 px-4 text-xs font-bold bg-blue-100 text-blue-600 border-r border-blue-600 rounded-3xl">
                        Link:
                      </div>
                      <div className="text-xs font-bold px-4 text-blue-400">
                        {`${window.location.origin}/view/${props.id}`}
                      </div>
                    </div>
                  )}
                </div>
              </div>
              <button
                type="button"
                className="flex w-full h-8 justify-center items-center rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                onClick={async () => {
                  await updateSharingSettings();
                }}
              >
                Update sharing settings
              </button>
            </div>
          </div>
        </ModalContent>
      </Modal>
      <Modal
        isOpen={deleteModalIsOpen}
        handleClose={() => {
          setDeleteModalIsOpen(false);
        }}
      >
        <ModalContent>
          <div className="flex flex-col justify-center items-center p-14">
            <div className="flex flex-col justify-between items-center">
              <div className="font-normal italic text-black w-full mx-4 text-center">
                Are you sure you want to delete this collection?
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
                    await removeCollection();
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
                    setDeleteModalIsOpen(false);
                  }}
                >
                  No
                </button>
              </div>
            </div>
          </div>
        </ModalContent>
      </Modal>
    </li>
  );
}
