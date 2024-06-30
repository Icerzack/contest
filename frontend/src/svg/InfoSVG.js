export default function InfoSVG(props) {
  return (
    <div>
      <svg
        width={props.width}
        height={props.height}
        viewBox="0 0 24 24"
        xmlns="http://www.w3.org/2000/svg"
        stroke="gray"
        fill="none"
      >
        <g id="SVGRepo_bgCarrier" strokeWidth="0" />
        <g
          id="SVGRepo_tracerCarrier"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <g id="SVGRepo_iconCarrier">
          <circle cx="12" cy="12" r="10" strokeWidth="1.5" />
          <path d="M12 17V11" strokeLinecap="round" strokeWidth="1.5" />
          <circle
            cx="1"
            cy="1"
            r="1"
            transform="matrix(1 0 0 -1 11 9)"
            fill="none"
          />
        </g>
      </svg>
    </div>
  );
}
