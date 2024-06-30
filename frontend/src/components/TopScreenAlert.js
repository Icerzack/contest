import { useRecoilState } from 'recoil';
import { topBarHeight } from '../state/atoms';

export default function TopScreenAlert() {
  const [barHeight] = useRecoilState(topBarHeight);

  return (
    <div
      className="bg-red-600 flex justify-center items-center text-indigo-100 max-w-full"
      role="alert"
      style={{ height: barHeight }}
    >
      {barHeight > 0 && (
        <div className="flex flex-row justify-center items-center">
          <div className="px-2 mr-5 p-0.5 h-5 text-xs font-medium tracking-wide text-red-500 bg-red-100 rounded-full text-center flex flex-row">
            WARNING ⚠️
          </div>
          <div className="font-normal text-sm mr-2 text-left flex-auto">
            Your session will expire in 5 minutes. Save your work, log out and
            log in again.
          </div>
        </div>
      )}
    </div>
  );
}
