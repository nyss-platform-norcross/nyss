import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as translationsActions from './logic/translationsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import TranslationsTable from './TranslationsTable';
import { useMount } from '../../utils/lifecycle';
import { Fragment } from 'react';
import { TranslationsFilters } from './TranslationsFilters';

const TranslationsListPageComponent = (props) => {
  useMount(() => {
    props.openTranslationsList(props.match.path, props.match.params);
  });

  return (
    <Fragment>
      <TranslationsFilters
        onChange={props.getTranslations}
      />
      <TranslationsTable
        isListFetching={props.isListFetching}
        languages={props.languages}
        translations={props.translations}
        type="strings"
      />
    </Fragment>
  );
}

TranslationsListPageComponent.propTypes = {
  isFetching: PropTypes.bool,
  languages: PropTypes.array,
  translations: PropTypes.array
};

const mapStateToProps = (state) => ({
  isListFetching: state.translations.listFetching,
  translations: state.translations.listTranslations,
  languages: state.translations.listLanguages
});

const mapDispatchToProps = {
  openTranslationsList: translationsActions.openTranslationsList.invoke,
  getTranslations: translationsActions.getTranslationsList.invoke
};

export const TranslationsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(TranslationsListPageComponent)
);
