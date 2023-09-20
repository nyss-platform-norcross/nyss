import { SvgIcon } from "@material-ui/core"
import { ReactComponent as CaseManagementSvg } from "../../assets/icons/Case-management.svg"
import { ReactComponent as HealthPostSvg } from "../../assets/icons/Health-post.svg"
import { ReactComponent as MonitorSvg } from "../../assets/icons/Monitor.svg"
import { ReactComponent as PeopleInNeed2Svg } from "../../assets/icons/People-in-need-2.svg"
import { ReactComponent as SettingsSvg } from "../../assets/icons/Settings.svg"
import { ReactComponent as UsersSvg } from "../../assets/icons/Users.svg"
import { ReactComponent as ValidateAccountSvg } from "../../assets/icons/Validate-account.svg"
import { ReactComponent as AlertSvg } from "../../assets/icons/Alert.svg"



export const RcIcon = ({ icon, ...props }) => {
  switch (icon) {
    case "Dashboard":
      return <SvgIcon {...props} component={MonitorSvg} viewBox="0 0 48 43" />
    case "Project":
      return <SvgIcon {...props} component={HealthPostSvg} viewBox="0 0 48 48" />
    case "Report":
      return <SvgIcon {...props} component={CaseManagementSvg} viewBox="0 0 32 48" />
    case "Users":
      return <SvgIcon {...props} component={PeopleInNeed2Svg} viewBox="0 0 48 32" />
    case "Settings":
      return <SvgIcon {...props} component={SettingsSvg} viewBox="0 0 48 48" />
    case "DataCollectors":
      return <SvgIcon {...props} component={ValidateAccountSvg} viewBox="0 0 48 48" />
    case "Alerts":
      return <SvgIcon {...props} component={AlertSvg} viewBox="0 0 48 41" />
    case "Users2":
      return <SvgIcon {...props} component={UsersSvg} viewBox="0 0 48 36" />
    default:
      return null
  }
}