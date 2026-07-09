import React from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as translationsActions from "./logic/translationsActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import TranslationsTable from "./TranslationsTable";
import { useMount } from "../../utils/lifecycle";
import { Fragment } from "react";
import { TranslationsFilters } from "./TranslationsFilters";
import { LinearProgress } from "@material-ui/core";
import { trackPageView } from "../../utils/appInsightsHelper";

const SmsTranslationsListPageComponent = (props) => {
  useMount(() => {
    props.openTranslationsList(props.match.path, props.match.params);

    // Track page view
    trackPageView("SmsTranslationsListPage");
  });

  return (
    <Fragment>
      <TranslationsFilters onChange={props.getSmsTranslations} />
      {props.isListFetching && <LinearProgress />}
      <TranslationsTable
        isListFetching={props.isListFetching}
        languages={props.languages}
        translations={props.translations}
        type="sms"
      />
    </Fragment>
  );
};

SmsTranslationsListPageComponent.propTypes = {
  isFetching: PropTypes.bool,
  languages: PropTypes.array,
  translations: PropTypes.array,
};

const mapStateToProps = (state) => ({
  isListFetching: state.translations.listFetching,
  translations: state.translations.smsTranslations,
  languages: state.translations.smsLanguages,
});

const mapDispatchToProps = {
  openTranslationsList: translationsActions.openSmsTranslationsList.invoke,
  getSmsTranslations: translationsActions.getSmsTranslationsList.invoke,
};

export const SmsTranslationsListPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(SmsTranslationsListPageComponent),
);
