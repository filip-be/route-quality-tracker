import { LatLng } from "leaflet";

type TrackPoint = LatLng;

type Track = {
  color: string;
  points: TrackPoint[];
};

export type GpxFile = {
  name: string;
  tracks: Track[];
};

export function parseGpx(gpxData: Document): GpxFile {
  const gpxNamespace = "http://www.topografix.com/GPX/1/1";
  const gpxStyleNamespace = "http://www.topografix.com/GPX/gpx_style/0/2";

  const metadataEl = gpxData.getElementsByTagNameNS(gpxNamespace, "metadata");
  const nameEl = metadataEl[0].getElementsByTagNameNS(gpxNamespace, "name");
  const name = nameEl[0].childNodes[0].nodeValue as string;

  const tracks = Array.from(
    gpxData.getElementsByTagNameNS(gpxNamespace, "trk")
  ).map((trackEl): Track => {
    const extensionsEl = trackEl.getElementsByTagNameNS(
      gpxNamespace,
      "extensions"
    );
    const colorEl =
      extensionsEl.length > 0
        ? extensionsEl[0].getElementsByTagNameNS(gpxStyleNamespace, "color")
        : undefined;
    const color =
      colorEl !== undefined && colorEl.length > 0
        ? `#${colorEl[0].childNodes[0].nodeValue as string}`
        : "#FFFFFF";

    const trackPoints = Array.from(
      trackEl.getElementsByTagNameNS(gpxNamespace, "trkpt")
    ).map((el): TrackPoint => {
      const eleElement = el.getElementsByTagNameNS(gpxNamespace, "ele");
      const latitudeString = el.getAttribute("lat")!;
      const longitudeString = el.getAttribute("lon")!;

      return new LatLng(
        Number.parseFloat(latitudeString),
        Number.parseFloat(longitudeString),
        eleElement?.length > 0
          ? Number.parseInt(eleElement[0].childNodes[0].nodeValue as string)
          : undefined
      );
    });

    return {
      color: color,
      points: trackPoints,
    };
  });

  return {
    name: name,
    tracks: tracks,
  };
}

export default parseGpx;
