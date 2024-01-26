import { useEffect, useState } from "react";
import "./App.css";
import { MapContainer, Polyline, TileLayer } from "react-leaflet";
import GpxFileContent from "./assets/track2.gpx?raw";
import { GpxFile, parseGpx } from "./GpxFileParser";

function App() {
  const [gpxFile, setGpxFile] = useState<GpxFile>();

  useEffect(() => {
    const gpxData = new DOMParser().parseFromString(
      GpxFileContent,
      "application/xml"
    );

    setGpxFile(parseGpx(gpxData));
  }, []);

  return (
    <>
      <h1>Route Quality</h1>
      {gpxFile ? (
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
      ) : (
        <>Loading...</>
      )}
    </>
  );
}

export default App;
