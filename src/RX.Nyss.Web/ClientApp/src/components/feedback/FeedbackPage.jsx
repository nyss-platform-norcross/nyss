import { withLayout } from '../../utils/layout';
import { Layout } from '../layout/Layout';

const Feedback = () => {
  return <>Please give feedback</>
};

export const FeedbackPage = withLayout(
  Layout,
  Feedback
);