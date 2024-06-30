export const getCurrentPageName = () => {
  const url = window.location.pathname;
  const regex = /^\/board\/\d+$/;
  if (url.match('personal')) {
    return '/personal';
  }
  if (url.match('shared')) {
    return '/shared';
  }
  if (regex.test(url)) {
    const boardId = url.substring(url.lastIndexOf('/') + 1);
    return `/board/${boardId}`;
  }
  return '/personal';
};
