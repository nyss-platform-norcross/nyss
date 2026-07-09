import React, { useState } from "react";
import PropTypes from "prop-types";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import { withLayout } from "../../utils/layout";
import { connect } from "react-redux";
import { AnonymousLayout } from "../layout/AnonymousLayout";
import styles from "./AgreementsPage.module.scss";
import { strings, stringKeys } from "../../strings";
import * as agreementsActions from "./logic/agreementsActions";
import { useMount } from "../../utils/lifecycle";
import {
  MenuItem,
  Grid,
  FormControl,
  InputLabel,
  Select,
  Snackbar,
  Button,
  Paper,
  Typography,
  Icon,
  Checkbox,
  FormControlLabel,
  CardMedia,
} from "@material-ui/core";
import { Loading } from "../common/loading/Loading";
import { trackPageView } from "../../utils/appInsightsHelper";

const AgreementsPageComponent = (props) => {
  const [hasConsented, setHasConsented] = useState(false);
  const [selectedLanguage, setSelectedLanguage] = useState("en");
  const [validationMessage, setValidationMessage] = useState(null);

  useMount(() => {
    props.openAgreementPage();

    // Track page view
    trackPageView("AgreementsPage");
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!hasConsented) {
      setValidationMessage(
        strings(stringKeys.nationalSocietyConsents.agreeToContinue),
      );
      return;
    }

    props.acceptAgreement(selectedLanguage);
  };

  const handleDocChange = (event) => {
    setSelectedLanguage(event.target.value);
  };

  const selectedDocumentUrl =
    props.agreementDocuments.length > 0 &&
    props.agreementDocuments.find((ad) => ad.languageCode === selectedLanguage)
      .agreementDocumentUrl;

  if (!props.pendingSocieties || !props.staleSocieties) {
    return <Loading />;
  }

  return (
    <div className={styles.consentWrapper}>
      <Paper className={styles.consentPaper}>
        <div className={styles.aboveDocument}>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Typography variant="h2">
                {strings(stringKeys.nationalSocietyConsents.title)}
              </Typography>

              {props.pendingSocieties.length > 0 && (
                <Typography variant="h6" gutterBottom>
                  {props.pendingSocieties.length > 1
                    ? strings(
                        stringKeys.nationalSocietyConsents.nationalSocieties,
                      )
                    : strings(
                        stringKeys.nationalSocietyConsents.nationalSociety,
                      )}
                  :{" "}
                  {props.pendingSocieties
                    .map((ns) => ns.nationalSocietyName)
                    .join(", ")}
                </Typography>
              )}

              {props.staleSocieties.length > 0 && (
                <Typography variant="h6" gutterBottom>
                  {props.staleSocieties.length > 1
                    ? strings(
                        stringKeys.nationalSocietyConsents
                          .nationalSocietiesWithUpdatedAgreement,
                      )
                    : strings(
                        stringKeys.nationalSocietyConsents
                          .nationalSocietyWithUpdatedAgreement,
                      )}
                  :{" "}
                  {props.staleSocieties
                    .map((ns) => ns.nationalSocietyName)
                    .join(", ")}
                </Typography>
              )}

              <Typography variant="body1">
                {strings(stringKeys.nationalSocietyConsents.consentText)}
              </Typography>
            </Grid>

            {props.agreementDocuments.length > 1 && (
              <Grid item xs={12} sm={4} lg={4}>
                <FormControl fullWidth>
                  <InputLabel shrink>
                    {strings(stringKeys.nationalSocietyConsents.selectLanguage)}
                  </InputLabel>
                  <Select onChange={handleDocChange} value={selectedLanguage}>
                    {props.agreementDocuments.map((ad) => (
                      <MenuItem key={ad.languageCode} value={ad.languageCode}>
                        {ad.language}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
            )}
          </Grid>
        </div>
        <div className={styles.agreementDocumentFrame}>
          {props.agreementDocuments.length > 0 && (
            <CardMedia
              className={styles.document}
              component="iframe"
              src={selectedDocumentUrl + "#toolbar=0&view=FitH"}
            />
          )}
        </div>
        <div className={styles.belowDocument}>
          <Grid container alignItems="center">
            {selectedDocumentUrl && (
              <Grid item xs>
                <a
                  target="_blank"
                  rel="noopener noreferrer"
                  href={selectedDocumentUrl}
                >
                  {strings(stringKeys.nationalSocietyConsents.downloadDirectly)}
                </a>
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
              <SubmitButton
                onClick={handleSubmit}
                isFetching={props.submitting}
              >
                {strings(stringKeys.nationalSocietyConsents.submit)}
              </SubmitButton>

              {props.staleSocieties.length > 0 &&
                props.pendingSocieties.length === 0 && (
                  <Button style={{ marginLeft: "15px" }} href="/">
                    {strings(stringKeys.nationalSocietyConsents.postpone)}
                  </Button>
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
        autoHideDuration={6000}
      />
    </div>
  );
};

AgreementsPageComponent.propTypes = {
  acceptAgreement: PropTypes.func,
};

const mapStateToProps = (state) => ({
  pendingSocieties: state.agreements.pendingSocieties,
  staleSocieties: state.agreements.staleSocieties,
  agreementDocuments: state.agreements.agreementDocuments,
  submitting: state.agreements.submitting,
});

const mapDispatchToProps = {
  openAgreementPage: agreementsActions.openAgreementPage.invoke,
  acceptAgreement: agreementsActions.acceptAgreement.invoke,
};

export const AgreementsPage = withLayout(
  AnonymousLayout,
  connect(mapStateToProps, mapDispatchToProps)(AgreementsPageComponent),
);
