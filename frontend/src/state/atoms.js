import { atom } from 'recoil';

export const selectedPageId = atom({
  key: 'selectedPageId',
  default: 'Personal',
});

export const user = atom({
  key: 'user',
  default: {},
});

export const themeState = atom({
  key: 'themeState',
  default: '',
});

export const progress = atom({
  key: 'progress',
  default: 0,
});

export const topBarHeight = atom({
  key: 'topBarHeight',
  default: 0,
});

export const personalCollectionsState = atom({
  key: 'personalCollectionsState',
  default: {},
});

export const isLoadingPersonalCollections = atom({
  key: 'isLoadingPersonalCollections',
  default: true,
});

export const personalPageNeedsUpdate = atom({
  key: 'personalPageNeedsUpdate',
  default: false,
});

export const sharedCollectionsState = atom({
  key: 'sharedCollectionsState',
  default: {},
});

export const sharedPersonalCollections = atom({
  key: 'sharedPersonalCollections',
  default: true,
});

export const sharedPageNeedsUpdate = atom({
  key: 'sharedPageNeedsUpdate',
  default: false,
});

export const isLoadingSharedCollections = atom({
  key: 'isLoadingSharedCollections',
  default: true,
});
