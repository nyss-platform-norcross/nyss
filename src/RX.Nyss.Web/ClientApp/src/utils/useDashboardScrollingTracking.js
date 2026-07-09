import { useSelector } from "react-redux";
import { useState, useEffect } from "react";
import { trackEvent } from "./appInsightsHelper";

// This code is used to track if a user is scrolling in either the project or NS dashboard.
const useDashboardScrollingTracking = () => {
  const [inDashboardPage, setInDashboardPage] = useState(false);
  const [hasScrolledInDashboard, setHasScrolledInDashboard] = useState(false);
  const pageTitle = useSelector(
    (state) => state.appData.siteMap.title,
  );
  const [scrollTop, setScrollTop] = useState(0);
  const hasScrolledThreshold = 150;

  useEffect(() => {
    if (pageTitle === "Dashboard") {
      !inDashboardPage && setInDashboardPage(true);
    } else {
      inDashboardPage && setInDashboardPage(false);
      hasScrolledInDashboard && setHasScrolledInDashboard(false);
    }
  }, [pageTitle]);

  useEffect(() => {
    if (inDashboardPage && scrollTop > hasScrolledThreshold && !hasScrolledInDashboard) {
      trackEvent("hasScrolledInDashboard");
      setHasScrolledInDashboard(true);
    }
  }, [scrollTop]);
  
  
  // Uses inDashboardPage and hasScrolledInDashboard state to not unneccesarrily store scrolling data.
  const handleScroll = (event) => {
    if(inDashboardPage && !hasScrolledInDashboard){
      setScrollTop(event.target.scrollTop);
    }
  }

  return handleScroll;
}

export default useDashboardScrollingTracking;