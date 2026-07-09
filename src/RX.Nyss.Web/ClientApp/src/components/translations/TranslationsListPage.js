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

const TranslationsListPageComponent = (props) => {
  useMount(() => {
    props.openTranslationsList(props.match.path, props.match.params);

    // Track page view
    trackPageView("TranslationsListPage");
  });

  return (
    <Fragment>
      <TranslationsFilters onChange={props.getTranslations} />
      {props.isListFetching && <LinearProgress />}
      <TranslationsTable
        isListFetching={props.isListFetching}
        languages={props.languages}
        translations={props.translations}
        type="strings"
      />
    </Fragment>
  );
};

TranslationsListPageComponent.propTypes = {
  isFetching: PropTypes.bool,
  languages: PropTypes.array,
  translations: PropTypes.array,
};

const mapStateToProps = (state) => ({
  isListFetching: state.translations.listFetching,
  translations: state.translations.listTranslations,
  languages: state.translations.listLanguages,
});

const mapDispatchToProps = {
  openTranslationsList: translationsActions.openTranslationsList.invoke,
  getTranslations: translationsActions.getTranslationsList.invoke,
};

export const TranslationsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(TranslationsListPageComponent),
);
