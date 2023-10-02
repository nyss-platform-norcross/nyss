import { useMount } from '../../utils/lifecycle';
import * as appActions from '../app/logic/appActions';
import { withLayout } from '../../utils/layout';
import { Layout } from '../layout/Layout';
import { connect } from "react-redux";

const Feedback = ({ openModule, match }) => {
  useMount(() => {
    openModule(match.path, match.params);
  });

  return <>Please give feedback</>
};

const mapDispatchToProps = {
  openModule: appActions.openModule.invoke
};

export const FeedbackPage = withLayout(
  Layout,
  connect(null, mapDispatchToProps)(Feedback)
);