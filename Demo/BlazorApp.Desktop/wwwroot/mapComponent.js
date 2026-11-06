// MapLibre GL (open-source fork of Mapbox GL, same Map API) with OpenFreeMap tiles: no access token needed.
const maplibreVersion = '4.7.1';
const maplibreBase = `https://unpkg.com/maplibre-gl@${maplibreVersion}/dist`;

// Load the stylesheet and the library once, before the first map is created (CSS first avoids MapLibre's
// "missing CSS" warning and a mis-sized canvas).
const maplibreReady = new Promise((resolve, reject) => {
  if (window.maplibregl) {
    resolve();
    return;
  }

  const css = document.createElement('link');
  css.rel = 'stylesheet';
  css.href = `${maplibreBase}/maplibre-gl.css`;
  document.head.appendChild(css);

  const script = document.createElement('script');
  script.src = `${maplibreBase}/maplibre-gl.js`;
  script.onload = () => resolve();
  script.onerror = () => reject(new Error(`Could not load ${script.src}`));
  document.head.appendChild(script);
});

// OpenFreeMap: free OpenStreetMap-based vector tiles, no key and no Referer needed (tile.openstreetmap.org
// blocks requests without a Referer, and pages on app://local send none).
const mapStyle = 'https://tiles.openfreemap.org/styles/liberty';

export async function addMapToElement(element) {
  await maplibreReady;
  return new maplibregl.Map({
    container: element,
    style: mapStyle,
    center: [-74.5, 40],
    zoom: 9
  });
}

export function setMapCenter(map, latitude, longitude) {
  map.setCenter([longitude, latitude]);
}
