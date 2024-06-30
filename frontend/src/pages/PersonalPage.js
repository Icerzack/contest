import { useRecoilState } from 'recoil';
import { useEffect, useRef, useState } from 'react';
import autoAnimate from '@formkit/auto-animate';
import { Loading } from 'react-daisyui';
import LeftSidebar from '../components/LeftSidebar';
import Collection from '../components/Collection';
import { getCollections, postCollection } from '../utils/api/collections';
import {
  isLoadingPersonalCollections,
  personalCollectionsState,
  personalPageNeedsUpdate,
  user,
} from '../state/atoms';
import Modal, { ModalContent } from '../components/Modal';
import { useAuth } from '../hooks/useAuth';

export default function PersonalPage() {
  const [collections, setCollections] = useRecoilState(
    personalCollectionsState,
  );
  const [isLoading, setIsLoading] = useRecoilState(
    isLoadingPersonalCollections,
  );
  const [needsUpdate, setNeedsUpdate] = useRecoilState(personalPageNeedsUpdate);
  const [modalIsOpen, setModalIsOpen] = useState(false);
  const [collectionToCreateName, setCollectionToCreateName] =
    useState('My Collection');
  const [currentUser] = useRecoilState(user);
  const [buttonDisabled, setButtonDisabled] = useState(false);
  const [showSaveUpdateLoader, setShowSaveUpdateLoader] = useState(false);
  const { token } = useAuth();
  const parentRef = useRef();

  useEffect(() => {
    if (parentRef.current) {
      autoAnimate(parentRef.current);
    }
  }, [parentRef]);

  const getAllCollections = async () => {
    getCollections(token).then((response) => {
      const filteredCollections = response.data.collections?.filter(
        (collection) => collection.authorId === currentUser.id,
      );
      setCollections({ collections: filteredCollections });
      setIsLoading(false);
    });
  };

  const createNewCollection = async () => {
    const collectionToCreate = {
      name: collectionToCreateName,
    };
    postCollection(collectionToCreate, token).then(() => {
      setModalIsOpen(false);
      setNeedsUpdate(true);
      setButtonDisabled(false);
      setShowSaveUpdateLoader(false);
    });
  };

  useEffect(() => {
    if (needsUpdate) {
      if (currentUser.id === undefined) {
        return;
      }
      setNeedsUpdate(false);
      getAllCollections();
    }
  });

  useEffect(() => {
    if (collections?.collections === undefined) {
      setIsLoading(true);
      setNeedsUpdate(true);
    }
  }, []);

  return (
    <div>
      <LeftSidebar>
        <div className="px-4 pt-4 bg-gray-50 flex flex-col h-full max-h-full w-full">
          <div className="pl-5 flex flex-row items-center justify-between z-10 h-20 transition-all duration-500">
            <div className="flex flex-col space-y-2">
              <h1 className="text-2xl font-bold italic text-white transition-all duration-500 intro-animation">
                Personal collections
              </h1>
              <h3 className="text-sm font-normal text-white intro-animation">
                Collections you created and shared with others
              </h3>
            </div>
            <button
              type="submit"
              className="btn-grad-2 px-3 py-1 rounded-2xl text-base text-white intro-animation"
              onClick={() => setModalIsOpen(true)}
            >
              Add Collection
            </button>
          </div>
          {isLoading ? (
            <div className="flex flex-col h-full w-full mt-14 justify-center items-center intro-animation">
              <Loading variant="dots" size="lg" />
              <div>Loading data...</div>
            </div>
          ) : (
            <div className="h-full overflow-x-hidden overflow-y-scroll mt-14">
              <div className="space-y-5 p-5">
                {collections?.collections?.length === 0 ? (
                  <div className="font-extralight italic text-black text-center mt-2 intro-animation">
                    No any collections
                  </div>
                ) : (
                  <ul className="space-y-7 intro-animation" ref={parentRef}>
                    {collections?.collections?.map((collection) => (
                      <Collection
                        key={collection.id}
                        id={collection.id}
                        name={collection.name}
                        boards={collection.boards}
                        isShared={collection.isShared}
                        displaySharedLabel
                        author={collection.authorId}
                        role={collection.role}
                      />
                    ))}
                  </ul>
                )}
              </div>
            </div>
          )}
        </div>
      </LeftSidebar>
      <Modal isOpen={modalIsOpen} handleClose={() => setModalIsOpen(false)}>
        <ModalContent>
          <div className="flex flex-col justify-center items-center space-y-10 p-16">
            <div className="flex flex-row justify-center items-center">
              <div className="font-light italic text-black flex-grow min-w-32">
                Collection name:
              </div>
              <input
                className="block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 flex-shrink"
                placeholder="My Collection"
                onChange={(event) => {
                  setCollectionToCreateName(event.target.value);
                }}
              />
            </div>
            <button
              type="button"
              className={`flex w-full justify-center items-center rounded-md bg-indigo-500 px-3 h-8 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-indigo-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600${
                buttonDisabled ? ' cursor-not-allowed disabled' : ''
              }`}
              onClick={async () => {
                if (buttonDisabled) {
                  return;
                }
                setButtonDisabled(true);
                setShowSaveUpdateLoader(true);
                await createNewCollection();
              }}
            >
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
                  <div className="">Create</div>
                )}
              </div>
            </button>
          </div>
        </ModalContent>
      </Modal>
    </div>
  );
}
