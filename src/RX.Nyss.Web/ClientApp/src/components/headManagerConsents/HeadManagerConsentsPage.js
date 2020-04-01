import React, { useState } from 'react';
import PropTypes from "prop-types";
import SubmitButton from '../forms/submitButton/SubmitButton';
import { useLayout } from '../../utils/layout';
import { connect } from "react-redux";
import { AnonymousLayout } from '../layout/AnonymousLayout';
import Paper from '@material-ui/core/Paper';
import Typography from '@material-ui/core/Typography';
import styles from './HeadManagerConsentsPage.module.scss';
import Icon from "@material-ui/core/Icon";
import { strings, stringKeys } from '../../strings';
import * as headManagerConsentsActions from './logic/headManagerConsentsActions';
import { useMount } from '../../utils/lifecycle';
import Checkbox from '@material-ui/core/Checkbox';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import CardMedia from '@material-ui/core/CardMedia';
import { MenuItem, Grid, FormControl, InputLabel, Select, Snackbar } from '@material-ui/core';

const HeadManagerConsentsPageComponent = (props) => {
  const [hasConsented, setHasConsented] = useState(false);
  const [selectedLanguage, setSelectedLanguage] = useState("en");
  const [validationMessage, setValidationMessage] = useState(null);

  useMount(() => {
    props.openHeadManagerConsentsPage();
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!hasConsented) {
      setValidationMessage(strings(stringKeys.headManagerConsents.agreeToContinue));
      return;
    };

    props.consentAsHeadManager(selectedLanguage);
  }

  const handleDocChange = (event) => {
    setSelectedLanguage(event.target.value);
  }

  const selectedDocumentUrl = props.agreementDocuments.length > 0 && props.agreementDocuments.find(ad => ad.languageCode === selectedLanguage).agreementDocumentUrl;

  return (
    <div className={styles.consentWrapper}>
      <Paper className={styles.consentPaper}>
        <div className={styles.aboveDocument}>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Typography variant="h2">{strings(stringKeys.headManagerConsents.title)}</Typography>

              <Typography variant="h6" gutterBottom>
                {props.nationalSocieties.length > 1 ?
                  strings(stringKeys.headManagerConsents.nationalSocieties) :
                  strings(stringKeys.headManagerConsents.nationalSociety)
                }: {props.nationalSocieties.map(ns => ns.nationalSocietyName).join(", ")}
              </Typography>

              <Typography variant="body1" >
                {strings(stringKeys.headManagerConsents.consentText)}
              </Typography>
            </Grid>

            {props.agreementDocuments.length > 1 &&
              (
                <Grid item xs={12} sm={4} lg={4}>
                  <FormControl fullWidth>
                    <InputLabel shrink>{strings(stringKeys.headManagerConsents.selectLanguage)}</InputLabel>
                    <Select
                      onChange={handleDocChange}
                      value={selectedLanguage}>
                      {props.agreementDocuments.map(ad => (<MenuItem key={ad.languageCode} value={ad.languageCode}>{ad.language}</MenuItem>))}
                    </Select>
                  </FormControl>
                </Grid>
              )}
          </Grid>
        </div>
        <div className={styles.agreementDocumentFrame}>
          {props.agreementDocuments.length > 0 &&
            <CardMedia className={styles.document} component="iframe" src={selectedDocumentUrl + '#toolbar=0&view=FitH'} />
          }
        </div>
        <div className={styles.belowDocument}>
          <Grid container alignItems="center">
            {selectedDocumentUrl &&
              (<Grid item xs>
                <a target="_blank" rel="noopener noreferrer" href={selectedDocumentUrl}>{strings(stringKeys.headManagerConsents.downloadDirectly)}</a>
              </Grid>
            )}
            <Grid item>
              <FormControlLabel
                control={
                  <Checkbox
                    onClick={() => {
                      setHasConsented(!hasConsented);
                      setValidationMessage(null);
                    }}
                    checked={hasConsented}
                    color="primary"
                  />
                }
                label={strings(stringKeys.headManagerConsents.iConsent)}
              />
              <SubmitButton onClick={handleSubmit} isFetching={props.submitting}>{strings(stringKeys.headManagerConsents.submit)}</SubmitButton>
            </Grid>
          </Grid>
        </div>
      </Paper>

      <Snackbar
        action={<Icon>close</Icon>}
        open={validationMessage}
        message={validationMessage}
        onClose={() => setValidationMessage(null)}
        onClick={() => setValidationMessage(null)}
        autoHideDuration={6000} />
    </div>
  );
}

HeadManagerConsentsPageComponent.propTypes = {
  consentAsHeadManager: PropTypes.func,
};

const mapStateToProps = state => ({
  nationalSocieties: state.headManagerConsents.nationalSocieties,
  agreementDocuments: state.headManagerConsents.agreementDocuments,
  submitting: state.headManagerConsents.submitting
});

const mapDispatchToProps = {
  openHeadManagerConsentsPage: headManagerConsentsActions.openHeadManagerConsentsPage.invoke,
  consentAsHeadManager: headManagerConsentsActions.consentAsHeadManager.invoke
};

export const HeadManagerConsentsPage = useLayout(
  AnonymousLayout,
  connect(mapStateToProps, mapDispatchToProps)(HeadManagerConsentsPageComponent)
);
