import { ChangeEvent, useEffect, useState } from "react";
import "./App.css";
import { MapContainer, Polyline, TileLayer } from "react-leaflet";
import { GpxFile, parseGpx } from "./GpxFileParser";

function App() {
  const [gpxFile, setGpxFile] = useState<GpxFile>();
  const [selectedFile, setSelectedFile] = useState<File>();
  const [gpxFileContent, setGpxFileContent] = useState<string>();

  useEffect(() => {
    if (selectedFile === undefined) {
      setGpxFileContent(undefined);
      setGpxFile(undefined);
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      setGpxFileContent(reader.result as string);
    };
    reader.readAsText(selectedFile);
  }, [selectedFile]);

  useEffect(() => {
    if (gpxFileContent === undefined) return;

    const gpxData = new DOMParser().parseFromString(
      gpxFileContent,
      "application/xml"
    );

    setGpxFile(parseGpx(gpxData));
  }, [gpxFileContent]);

  function changeGpxFile(event: ChangeEvent<HTMLInputElement>): void {
    const selectedFile = event.target.files?.[0];
    console.log(`selected file: ${selectedFile?.name}`);
    setSelectedFile(selectedFile);
  }

  return (
    <>
      <h1>Route Quality</h1>
      <div>
        <p>Select file to render:</p>
        <input type="file" onChange={changeGpxFile} />
      </div>
      {gpxFile && (
        <>
          <h2>{gpxFile.name}</h2>
          <MapContainer
            id="map"
            center={gpxFile.tracks[0].points[0]}
            zoom={10}
            scrollWheelZoom={true}
          >
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />

            {gpxFile.tracks.map((t, index) => {
              return (
                <Polyline
                  key={`track${index}`}
                  positions={t.points}
                  pathOptions={{
                    color: t.color,
                  }}
                />
              );
            })}
          </MapContainer>
        </>
      )}
    </>
  );
}

export default App;
