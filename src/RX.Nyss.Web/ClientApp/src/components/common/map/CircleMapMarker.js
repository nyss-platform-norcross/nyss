import { CircleMarker as LeafletCircleMarker } from 'leaflet'
import { withLeaflet, Path } from 'react-leaflet';

LeafletCircleMarker.include({
  setLatLng: function (latlng) {
    const oldLatLng = this._latlng;
    this._update();
    return this.fire('move', {oldLatLng: oldLatLng, latlng: this._latlng});
  },
})

class CircleMapMarker extends Path {
  createLeafletElement({ position, leaflet, ...options }) {
    return new LeafletCircleMarker(position, options);
  }

  updateLeafletElement(fromProps, { position, radius }) {
    if (position !== fromProps.position) {
      this.leafletElement.setLatLng(position);
    }
    if (radius !== fromProps.radius) {
      this.leafletElement.setRadius(radius);
    }
  }
}

export default withLeaflet(CircleMapMarker);