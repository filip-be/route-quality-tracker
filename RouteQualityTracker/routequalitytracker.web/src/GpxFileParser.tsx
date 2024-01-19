import { useEffect, useState } from "react";

type TrackPoint = {
  lat: string;
  lon: string;
  ele: number;
};

type Track = {
  color: string;
  points: TrackPoint[];
};

export type GpxFile = {
  name: string;
  Tracks: Track[];
};

export function parseGpx(gpxData: Document): GpxFile {
  const gpxNamespace = "http://www.topografix.com/GPX/1/1";
  const gpxStyleNamespace = "http://www.topografix.com/GPX/gpx_style/0/2";

  const metadataEl = gpxData.getElementsByTagNameNS(gpxNamespace, "metadata");
  const nameEl = metadataEl[0].getElementsByTagNameNS(gpxNamespace, "name");
  const name = nameEl[0].childNodes[0].nodeValue as string;

  const tracks = Array.from(
    gpxData.getElementsByTagNameNS(gpxNamespace, "trk")
  ).map((trackEl) => {
    const extensionsEl = trackEl.getElementsByTagNameNS(
      gpxNamespace,
      "extensions"
    );
    const colorEl;
  });
  tracks.map;
  //   .map((trackEl) => {

  //   });

  return {
    name: name,
  };
}

export default parseGpx;
