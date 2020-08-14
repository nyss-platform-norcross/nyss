import React, { useState } from 'react';
import PropTypes from "prop-types";
import SubmitButton from '../forms/submitButton/SubmitButton';
import { useLayout } from '../../utils/layout';
import { connect } from "react-redux";
import { AnonymousLayout } from '../layout/AnonymousLayout';
import Paper from '@material-ui/core/Paper';
import Typography from '@material-ui/core/Typography';
import styles from './NationalSocietyConsentsPage.module.scss';
import Icon from "@material-ui/core/Icon";
import { strings, stringKeys } from '../../strings';
import * as nationalSocietyConsentsActions from './logic/nationalSocietyConsentsActions';
import { useMount } from '../../utils/lifecycle';
import Checkbox from '@material-ui/core/Checkbox';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import CardMedia from '@material-ui/core/CardMedia';
import { MenuItem, Grid, FormControl, InputLabel, Select, Snackbar, Button } from '@material-ui/core';

const NationalSocietyConsentsPageComponent = (props) => {
  const [hasConsented, setHasConsented] = useState(false);
  const [selectedLanguage, setSelectedLanguage] = useState("en");
  const [validationMessage, setValidationMessage] = useState(null);

  useMount(() => {
    props.openNationalSocietyConsentsPage();
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!hasConsented) {
      setValidationMessage(strings(stringKeys.nationalSocietyConsents.agreeToContinue));
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
              <Typography variant="h2">{strings(stringKeys.nationalSocietyConsents.title)}</Typography>

              {(props.pendingSocieties.length > 0 &&
                <Typography variant="h6" gutterBottom>
                  {props.pendingSocieties.length > 1 ?
                    strings(stringKeys.nationalSocietyConsents.nationalSocieties) :
                    strings(stringKeys.nationalSocietyConsents.nationalSociety)
                  }: {props.pendingSocieties.map(ns => ns.nationalSocietyName).join(", ")}
                </Typography>
              )}

              {(props.staleSocieties.length > 0 &&
                <Typography variant="h6" gutterBottom>
                  {props.staleSocieties.length > 1 ?
                    strings(stringKeys.nationalSocietyConsents.nationalSocieties) :
                    strings(stringKeys.nationalSocietyConsents.nationalSociety)
                  }: {props.staleSocieties.map(ns => ns.nationalSocietyName).join(", ")}
                </Typography>
              )}

              <Typography variant="body1" >
                {strings(stringKeys.nationalSocietyConsents.consentText)}
              </Typography>
            </Grid>

            {props.agreementDocuments.length > 1 &&
              (
                <Grid item xs={12} sm={4} lg={4}>
                  <FormControl fullWidth>
                    <InputLabel shrink>{strings(stringKeys.nationalSocietyConsents.selectLanguage)}</InputLabel>
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
                <a target="_blank" rel="noopener noreferrer" href={selectedDocumentUrl}>{strings(stringKeys.nationalSocietyConsents.downloadDirectly)}</a>
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
                label={strings(stringKeys.nationalSocietyConsents.iConsent)}
              />
              <SubmitButton onClick={handleSubmit} isFetching={props.submitting}>{strings(stringKeys.nationalSocietyConsents.submit)}</SubmitButton>

              {(props.staleSocieties.length > 0 && props.pendingSocieties.length === 0 &&
                <Button style={{marginLeft: "15px"}} href="/">{strings(stringKeys.nationalSocietyConsents.postpone)}</Button>
              )}
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

NationalSocietyConsentsPageComponent.propTypes = {
  consentAsHeadManager: PropTypes.func,
};

const mapStateToProps = state => ({
  pendingSocieties: state.nationalSocietyConsents.pendingSocieties,
  staleSocieties: state.nationalSocietyConsents.staleSocieties,
  agreementDocuments: state.nationalSocietyConsents.agreementDocuments,
  submitting: state.nationalSocietyConsents.submitting
});

const mapDispatchToProps = {
  openNationalSocietyConsentsPage: nationalSocietyConsentsActions.openNationalSocietyConsentsPage.invoke,
  consentAsHeadManager: nationalSocietyConsentsActions.consentAsHeadManager.invoke
};

export const NationalSocietyConsentsPage = useLayout(
  AnonymousLayout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyConsentsPageComponent)
);
