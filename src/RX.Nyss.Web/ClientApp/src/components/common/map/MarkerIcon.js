import styles from "./MarkerIcon.module.scss"
import L from 'leaflet'

export const SignIcon = ({ icon, className, size, multiple }) => L.divIcon({
  html: `<span class="material-icons">${icon}</span>`,
  className: `${className} ${styles.marker} ${multiple ? styles.multiple : null}`,
  iconSize: L.point(size, size, true),
});

export const TextIcon = ({ text, size, multiple }) => L.divIcon({
  html: `${text}`,
  className: `${styles.text} ${styles.marker} ${multiple ? styles.multiple : null}`,
  iconSize: L.point(size, size, true),
});