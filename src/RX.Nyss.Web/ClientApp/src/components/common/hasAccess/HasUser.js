import { connect } from 'react-redux';

const mapStateToProps = state => ({
  user: state.appData.user
});

export const useUser = (Component) => connect(mapStateToProps)(Component);
