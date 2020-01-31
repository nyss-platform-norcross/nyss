import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as translationsActions from './logic/translationsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import TranslationsTable from './TranslationsTable';
import { useMount } from '../../utils/lifecycle';

const TranslationsListPageComponent = (props) => {
  useMount(() => {
    props.openTranslationsList(props.match.path, props.match.params);
  });

  return (
    <Fragment>
      <TranslationsTable
        list={props.list}
        isListFetching={props.isListFetching}
      />
    </Fragment>
  );
}

TranslationsListPageComponent.propTypes = {
  getTranslations: PropTypes.func,
  isFetching: PropTypes.bool,
  //list: PropTypes.object
};

const mapStateToProps = (state, ownProps) => ({
  list: state.translations.listData,
  isListFetching: state.translations.listFetching
});

const mapDispatchToProps = {
  openTranslationsList: translationsActions.openList.invoke
};

export const TranslationsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(TranslationsListPageComponent)
);
