import { useEffect, useState } from "react";
import "./App.css";
import { MapContainer, Marker, Popup, TileLayer } from "react-leaflet";
import GpxFileContent from "./assets/track.gpx?raw";
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
            center={[51.505, -0.09]}
            zoom={10}
            scrollWheelZoom={false}
          >
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />

            <Marker position={[51.505, -0.09]}>
              <Popup>
                A pretty CSS3 popup. <br /> Easily customizable.
              </Popup>
            </Marker>
          </MapContainer>
        </>
      ) : (
        <>Loading...</>
      )}
    </>
  );
}

export default App;
