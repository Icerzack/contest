import { Link, useNavigate } from 'react-router-dom';
import { useRecoilState } from 'recoil';
import { useEffect } from 'react';
import { Divider } from 'react-daisyui';
import { selectedPageId, topBarHeight, user } from '../state/atoms';
import PersonalSVG from '../svg/PersonalSVG';
import SharedSVG from '../svg/SharedSVG';
import { useAuth } from '../hooks/useAuth';
import ProfileSVG from '../svg/ProfileSVG';
import { getAboutMe } from '../utils/api/users';
// import { useEffect } from "react";

export default function LeftSidebar(props) {
  const [, setSelectedId] = useRecoilState(selectedPageId);
  const [barHeight] = useRecoilState(topBarHeight);
  const navigate = useNavigate();
  const [currentUser, setCurrentUser] = useRecoilState(user);
  const { token, logout } = useAuth();

  useEffect(() => {
    if (currentUser.id === undefined) {
      getAboutMe(token).then((response) => {
        setCurrentUser({ id: response.data.id, name: response.data.name });
      });
    }
  }, []);

  // eslint-disable-next-line react/no-unstable-nested-components
  function PagesList(opts) {
    const { pages } = opts;
    const pagesItems = pages.map((page) => (
      <Link to={page.path} key={page.name}>
        <div
          className="relative flex flex-row items-center h-11 focus:outline-none hover:bg-blue-50 hover:bg-opacity-75 text-gray-600 hover:text-gray-800 border-transparent pr-6 rounded-3xl mx-1 hover:scale-105 transition-transform duration-500"
          key={page.name}
          onClick={() => {
            setSelectedId(page.name);
          }}
        >
          <span className="inline-flex justify-center items-center ml-4">
            <div>{page.icon}</div>
          </span>
          <span className="ml-2 text-sm tracking-wide truncate text-gray-600">
            {page.name}
          </span>
          {page.labelColor === 'green' ? (
            <span className="px-2 py-0.5 ml-auto text-xs font-medium tracking-wide text-green-500 bg-green-50 rounded-full">
              {page.labelText}
            </span>
          ) : (
            <div />
          )}
          {page.labelColor === 'indigo' ? (
            <span className="px-2 py-0.5 ml-auto text-xs font-medium tracking-wide text-indigo-500 bg-indigo-50 rounded-full">
              {page.labelText}
            </span>
          ) : (
            <div />
          )}
          {page.labelColor === 'orange' ? (
            <span className="px-2 py-0.5 ml-auto text-xs font-medium tracking-wide text-orange-500 bg-orange-50 rounded-full text-center">
              {page.labelText}
            </span>
          ) : (
            <div />
          )}
          {page.labelColor === 'beta' ? (
            <span className="px-2 h-5 ml-auto text-xs font-medium tracking-wide text-green-600 border border-green-600 rounded-full text-center flex justify-center items-center">
              {page.labelText}
            </span>
          ) : (
            <div />
          )}
        </div>
      </Link>
    ));
    return <ul>{pagesItems}</ul>;
  }

  useEffect(() => {
    const url = window.location.href;
    if (url.includes('personal')) {
      setSelectedId('Personal');
    }
    if (url.includes('shared')) {
      setSelectedId('Shared');
    }
  }, []);

  const h = window.innerHeight;
  const contentHeight = h - barHeight;

  return (
    <div className="">
      <div
        className="gradient absolute h-28 top-0 right-0 left-0"
        style={{ top: barHeight }}
      />
      <div
        className="flex flex-row bg-gray-50"
        style={{ height: contentHeight }}
      >
        <div
          className="flex z-10 min-w-60 shrink-0 bg-white flex-col rounded-3xl border-t border-l border-b ml-6 mb-6 mt-6 box-border"
          style={{
            boxShadow: '0px 0px 15px 0px #bbb',
            borderColor: '#eee',
          }}
        >
          <div className="flex flex-col h-full border-r rounded-r-3xl">
            <div className="flex flex-col justify-center h-10 border-b-4 border-blue-300 rounded-3xl font-light italic my-2">
              <div className="ml-2 text-gray-600 pl-4 pb-1">
                {currentUser?.name
                  ? `Username: ${currentUser?.name}`
                  : 'Username: N/A'}
              </div>
            </div>
            <button
              type="button"
              className="btn-grad"
              onClick={() => {
                navigate('/board', { replace: true });
              }}
            >
              New Board
            </button>
            <div className="overflow-y-auto overflow-x-hidden flex-grow">
              <ul className="flex flex-col py-4 space-y-1">
                <li className="px-5">
                  <Divider>
                    <div className="text-sm font-light tracking-wide text-gray-500">
                      Menu
                    </div>
                  </Divider>
                </li>
                <PagesList
                  pages={[
                    {
                      name: 'Personal',
                      path: '/personal',
                      icon: <PersonalSVG width="24" height="24" />,
                    },
                    {
                      name: 'Shared',
                      path: '/shared',
                      icon: <SharedSVG width="24" height="24" />,
                      labelColor: 'beta',
                      labelText: 'Beta',
                    },
                  ]}
                />
                <li className="px-5">
                  <Divider>
                    <div className="text-sm font-light tracking-wide text-gray-500">
                      Settings
                    </div>
                  </Divider>
                </li>
                <PagesList
                  pages={[
                    {
                      name: 'Profile',
                      path: '/personal',
                      icon: (
                        <ProfileSVG width="24" height="24" stroke="orange" />
                      ),
                      labelColor: 'orange',
                      labelText: 'soon',
                    },
                  ]}
                />
              </ul>
            </div>
            <div className="flex flex-col space-y-2 mb-3">
              <div
                className="relative flex mb-3 hover:scale-105 transition-transform duration-500 flex-row items-center h-11 focus:outline-none hover:bg-red-50 text-gray-600 hover:text-gray-800 border-l-4 border-transparent rounded-3xl pr-6 hover:cursor-pointer mx-1"
                onClick={() => {
                  logout();
                  window.location.reload();
                }}
              >
                <span className="inline-flex justify-center items-center ml-4">
                  <svg
                    className="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                    xmlns="http://www.w3.org/2000/svg"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth="2"
                      stroke="red"
                      d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"
                    />
                  </svg>
                </span>
                <span className="ml-2 text-sm tracking-wide truncate text-red-500">
                  Logout
                </span>
              </div>
            </div>
          </div>
        </div>
        <div className="flex-auto">{props.children}</div>
      </div>
    </div>
  );
}
