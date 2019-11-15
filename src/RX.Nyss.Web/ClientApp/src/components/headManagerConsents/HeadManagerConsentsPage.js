import React, { useState } from 'react';
import PropTypes from "prop-types";
import SubmitButton from '../forms/submitButton/SubmitButton';
import Form from '../forms/form/Form';
import { useLayout } from '../../utils/layout';
import { connect } from "react-redux";
import { AnonymousLayout } from '../layout/AnonymousLayout';
import Paper from '@material-ui/core/Paper';
import Typography from '@material-ui/core/Typography';
import styles from './HeadManagerConsentsPage.module.scss';
import { strings, stringKeys } from '../../strings';
import * as headManagerConsentsActions from './logic/headManagerConsentsActions';
import { useMount } from '../../utils/lifecycle';
import { Checkbox, FormGroup, FormControlLabel, SnackbarContent } from '@material-ui/core';

const HeadManagerConsentsPageComponent = (props) => {
  const [hasConsented, consent] = useState(false);
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

    props.consentAsHeadManager();
  };

  return (
    <div className={styles.consentContent}>
      <Paper className={styles.consentPaper}>
        <div className={styles.consentPaperContent}>
          <Typography variant="h2">{strings(stringKeys.headManagerConsents.title)}</Typography>

          {props.consentAsHeadManagerErrorMessage && <SnackbarContent message={props.consentAsHeadManagerErrorMessage} />}
          {validationMessage && <SnackbarContent message={validationMessage} />}

          <Typography variant="body1">
            {strings(stringKeys.headManagerConsents.consentText)}
          </Typography>
          <Typography variant="body1">{`${props.nationalSocieties.length > 1 ?
            strings(stringKeys.headManagerConsents.nationalSocieties) :
            strings(stringKeys.headManagerConsents.nationalSociety)}: `}
              {props.nationalSocieties.map(ns => `${ns.nationalSocietyName} (${ns.nationalSocietyCountryName})`).join(', ')}
          </Typography>

          <Form onSubmit={handleSubmit} fullWidth>
            <FormGroup row>
              <FormControlLabel
                control={
                  <Checkbox
                    onClick={() => {
                      consent(!hasConsented);
                      setValidationMessage(null);
                    }}
                    checked={hasConsented}
                    color="primary"
                  />
                }
                label={strings(stringKeys.headManagerConsents.iConsent)}
              />
            </FormGroup>
            <div className={styles.actions}>
              <SubmitButton isFetching={props.submitting}>{strings(stringKeys.headManagerConsents.submit)}</SubmitButton>
            </div>
          </Form>
        </div>
      </Paper>
    </div>
  );
}

HeadManagerConsentsPageComponent.propTypes = {
  consentAsHeadManager: PropTypes.func,
  consentAsHeadManagerErrorMessage: PropTypes.string
};

const mapStateToProps = state => ({
  consentAsHeadManagerErrorMessage: state.headManagerConsents.consentAsHeadManagerErrorMessage,
  nationalSocieties: state.headManagerConsents.nationalSocieties,
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
