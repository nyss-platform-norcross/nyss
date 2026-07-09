export const calculateBounds = (data) => {
  function getMinLat(points) {
    return points.reduce(
      (min, p) => (p.lat < min ? p.lat : min),
      points[0].lat,
    );
  }
  function getMaxLat(points) {
    return points.reduce(
      (max, p) => (p.lat > max ? p.lat : max),
      points[0].lat,
    );
  }
  function getMinLng(points) {
    return points.reduce(
      (min, p) => (p.lng < min ? p.lng : min),
      points[0].lng,
    );
  }
  function getMaxLng(points) {
    return points.reduce(
      (max, p) => (p.lng > max ? p.lng : max),
      points[0].lng,
    );
  }

  let points = data.map((loc) => ({
    lat: loc.location.latitude,
    lng: loc.location.longitude,
  }));
  return [
    [getMinLat(points), getMinLng(points)],
    [getMaxLat(points), getMaxLng(points)],
  ];
};

export const calculateCenter = (data) =>
  data
    .map((l) => [l.lat, l.lng])
    .reduce(
      (avg, value, _, { length }) => {
        return [avg[0] + value[0] / length, avg[1] + value[1] / length];
      },
      [0, 0],
    );

export const retrieveGpsLocation = (callback) => {
  if (!navigator.geolocation) {
    throw new Error("Geolocation is not supported by this browser");
  }

  return navigator.geolocation.getCurrentPosition(callback, (e) => {
    callback(null);
  });
};

export const calculateIconSize = (count, total) => {
  const minSize = 30,
    maxSize = 130;
  const size = Math.log10(count, total) * 40;

  if (size < minSize) {
    return minSize;
  } else if (size > maxSize) {
    return maxSize;
  } else {
    return size;
  }
};
