import { useState } from "react";
import { trackEvent } from "./appInsightsHelper";

const useHoverChartTracking = () => {
  const [hasHoveredChart, setHasHoveredChart] = useState(false);
  const trackHoveredChart = (chartName) => {
    if (!hasHoveredChart){
      trackEvent(chartName);
      setHasHoveredChart(true)};
    };

  return trackHoveredChart;
};

export default useHoverChartTracking;
