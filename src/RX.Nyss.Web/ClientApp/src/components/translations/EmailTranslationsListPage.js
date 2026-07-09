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

const EmailTranslationsListPageComponent = (props) => {
  useMount(() => {
    props.openTranslationsList(props.match.path, props.match.params);

    // Track page view
    trackPageView("EmailTranslationsListPage");
  });

  return (
    <Fragment>
      <TranslationsFilters onChange={props.getEmailTranslations} />
      {props.isListFetching && <LinearProgress />}
      <TranslationsTable
        isListFetching={props.isListFetching}
        languages={props.languages}
        translations={props.translations}
        type="email"
      />
    </Fragment>
  );
};

EmailTranslationsListPageComponent.propTypes = {
  isFetching: PropTypes.bool,
  languages: PropTypes.array,
  translations: PropTypes.array,
};

const mapStateToProps = (state) => ({
  isListFetching: state.translations.listFetching,
  translations: state.translations.emailTranslations,
  languages: state.translations.emailLanguages,
});

const mapDispatchToProps = {
  openTranslationsList: translationsActions.openEmailTranslationsList.invoke,
  getEmailTranslations: translationsActions.getEmailTranslationsList.invoke,
};

export const EmailTranslationsListPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(EmailTranslationsListPageComponent),
);
