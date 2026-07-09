import { SvgIcon } from "@material-ui/core";
import { ReactComponent as CaseManagementSvg } from "../../assets/icons/Case-management.svg";
import { ReactComponent as HealthPostSvg } from "../../assets/icons/Health-post.svg";
import { ReactComponent as MonitorSvg } from "../../assets/icons/Monitor.svg";
import { ReactComponent as PeopleInNeed2Svg } from "../../assets/icons/People-in-need-2.svg";
import { ReactComponent as SettingsSvg } from "../../assets/icons/Settings.svg";
import { ReactComponent as UserCircleSvg } from "../../assets/icons/Account-circle.svg";
import { ReactComponent as LogoutSvg } from "../../assets/icons/Logout.svg";
import { ReactComponent as FeedbackSvg } from "../../assets/icons/Feedback.svg";
import { ReactComponent as CovidSvg } from "../../assets/icons/COVID-19.svg";
import { ReactComponent as UrbanSvg } from "../../assets/icons/Urban.svg";
import { ReactComponent as RemoteSupportSvg } from "../../assets/icons/Remote-support.svg";
import { ReactComponent as AlertsSvg } from "../../assets/icons/Alerts.svg";
import { ReactComponent as CollapseSvg } from "../../assets/icons/collapse.svg";
import { ReactComponent as ExpandSvg } from "../../assets/icons/expand.svg";

export const RcIcon = ({ icon, ...props }) => {
  switch (icon) {
    case "Dashboard":
      return <SvgIcon {...props} component={MonitorSvg} viewBox="0 0 48 43" />;
    case "Project":
      return (
        <SvgIcon {...props} component={HealthPostSvg} viewBox="0 0 48 48" />
      );
    case "Report":
      return (
        <SvgIcon {...props} component={CaseManagementSvg} viewBox="0 0 32 48" />
      );
    case "Users":
      return (
        <SvgIcon {...props} component={PeopleInNeed2Svg} viewBox="0 0 48 32" />
      );
    case "Settings":
      return <SvgIcon {...props} component={SettingsSvg} viewBox="0 0 48 48" />;
    case "UserCircle":
      return (
        <SvgIcon {...props} component={UserCircleSvg} viewBox="0 0 22 21" />
      );
    case "Logout":
      return <SvgIcon {...props} component={LogoutSvg} viewBox="0 0 22 22" />;
    case "Feedback":
      return <SvgIcon {...props} component={FeedbackSvg} viewBox="0 0 20 19" />;
    case "HealthRisks":
      return <SvgIcon {...props} component={CovidSvg} viewBox="0 0 22 20" />;
    case "NationalSocieties":
      return <SvgIcon {...props} component={UrbanSvg} viewBox="0 0 22 20" />;
    case "GlobalCoordinators":
      return (
        <SvgIcon {...props} component={RemoteSupportSvg} viewBox="0 0 22 20" />
        );
    case "Alerts":
      return <SvgIcon {...props} component={AlertsSvg} viewBox="0 0 38 48" />;
    case "Expand":
      return <SvgIcon {...props} component={ExpandSvg} viewBox="0 0 38 38" />;
    case "Collapse":
      return <SvgIcon {...props} component={CollapseSvg} viewBox="0 0 39 38" />;
    default:
      return null;
  }
};
