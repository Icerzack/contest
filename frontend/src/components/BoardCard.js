export default function BoardCard(props) {
  const image = props.picture;
  const appStateParsed = JSON.parse(props.appState);
  return (
    <div
      className="box-border bg-white border border-gray-200 rounded-3xl hover:scale-105 transition hover:cursor-pointer"
      style={{
        width: '200px',
        transition: 'all 0.3s ease-in-out',
        background: '#ffffff',
        boxShadow: '0px 0px 10px 2px #eee',
        borderColor: '#eee',
      }}
      onClick={() => {
        props.select(props.id);
      }}
    >
      {image ? (
        <div
          className="flex justify-center items-center rounded-3xl"
          style={{
            backgroundColor: appStateParsed.viewBackgroundColor,
            borderBottom: '1px solid #eee',
          }}
        >
          <img
            src={`data:image/svg+xml;utf8,${encodeURIComponent(image)}`}
            style={{ width: '200px', height: '200px' }}
            className="rounded-3xl"
            alt="img"
          />
        </div>
      ) : (
        <div
          className="flex justify-center items-center"
          style={{
            width: '200px',
            height: '200px',
            borderBottom: '1px solid #eee',
          }}
        >
          {"Can't fetch image"}{' '}
        </div>
      )}
      <div className="p-5">
        <h5 className="mb-2 text-lg font-normal tracking-tight text-black">
          {props.name}
        </h5>
      </div>
    </div>
  );
}
