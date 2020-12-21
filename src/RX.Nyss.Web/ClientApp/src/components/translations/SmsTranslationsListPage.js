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

const SmsTranslationsListPageComponent = (props) => {
  useMount(() => {
    props.openTranslationsList(props.match.path, props.match.params);
  });

  return (
    <Fragment>
      <TranslationsFilters
        onChange={props.getSmsTranslations}
      />
      <TranslationsTable
        isListFetching={props.isListFetching}
        languages={props.languages}
        translations={props.translations}
        type="sms"
      />
    </Fragment>
  );
}

SmsTranslationsListPageComponent.propTypes = {
  isFetching: PropTypes.bool,
  languages: PropTypes.array,
  translations: PropTypes.array
};

const mapStateToProps = (state) => ({
  isListFetching: state.translations.listFetching,
  translations: state.translations.smsTranslations,
  languages: state.translations.smsLanguages
});

const mapDispatchToProps = {
  openTranslationsList: translationsActions.openSmsTranslationsList.invoke,
  getSmsTranslations: translationsActions.getSmsTranslationsList.invoke
};

export const SmsTranslationsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(SmsTranslationsListPageComponent)
);
