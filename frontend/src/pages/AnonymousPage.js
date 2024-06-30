import { useEffect, useState } from 'react';
import Collection from '../components/Collection';
import { getCollectionById } from '../utils/api/collections';

export default function AnonymousPage() {
  const [collection, setCollection] = useState();

  useEffect(() => {
    const collectionId = window.location.pathname.split('/').pop();
    getCollectionById(collectionId).then((response) => {
      setCollection(response.data.collection);
    });
  }, []);

  return (
    <div>
      <Collection
        id={collection.id}
        name={collection.name}
        boards={collection.boards}
        isShared={collection.isShared}
        displaySharedLabel
        author={collection.authorId}
        role={collection.role}
      />
    </div>
  );
}
