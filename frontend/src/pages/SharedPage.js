import { useRecoilState } from 'recoil';
import { useEffect, useRef } from 'react';
import autoAnimate from '@formkit/auto-animate';
import { Loading } from 'react-daisyui';
import LeftSidebar from '../components/LeftSidebar';
import Collection from '../components/Collection';
import { getCollections } from '../utils/api/collections';
import {
  isLoadingSharedCollections,
  sharedCollectionsState,
  sharedPageNeedsUpdate,
  user,
} from '../state/atoms';
import { useAuth } from '../hooks/useAuth';

export default function SharedPage() {
  const [collections, setCollections] = useRecoilState(sharedCollectionsState);
  const [isLoading, setIsLoading] = useRecoilState(isLoadingSharedCollections);
  const [needsUpdate, setNeedsUpdate] = useRecoilState(sharedPageNeedsUpdate);
  const { token } = useAuth();
  const [currentUser] = useRecoilState(user);
  const parentRef = useRef();

  useEffect(() => {
    if (parentRef.current) {
      autoAnimate(parentRef.current);
    }
  }, [parentRef]);

  const getAllSharedCollections = async () => {
    getCollections(token).then((response) => {
      const filteredCollections = response.data.collections?.filter(
        (collection) => collection.authorId !== currentUser.id,
      );
      setCollections({ collections: filteredCollections });
      setIsLoading(false);
    });
  };

  useEffect(() => {
    if (needsUpdate) {
      if (currentUser.id === undefined) {
        return;
      }
      setNeedsUpdate(false);
      getAllSharedCollections();
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
          <div className="pl-5 flex flex-row items-center justify-between z-10 h-20 intro-animation">
            <div className="flex flex-col space-y-2">
              <h1 className="text-2xl font-bold italic text-white transition-all duration-500 intro-animation">
                Shared collections
              </h1>
              <h3 className="text-sm font-normal text-white intro-animation">
                Collections which are shared with you
              </h3>
            </div>
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
                        displaySharedLabel={false}
                        displayAuthorLabel
                        author={collection.authorId}
                        isShared={collection.isShared}
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
    </div>
  );
}
