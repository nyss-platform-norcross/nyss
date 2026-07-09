import {
  Button,
  Card,
  CardContent,
  Grid,
  Typography,
  makeStyles,
} from "@material-ui/core";
import { Fragment, createRef, useMemo, useState } from "react";
import { createForm, validators } from "../../../utils/forms";
import TextInputField from "../../forms/TextInputField";
import { strings, stringKeys } from "../../../strings";
import EditIcon from "@material-ui/icons/Edit";
import Form from "../../forms/form/Form";

const useStyle = makeStyles((theme) => ({
  card: {
    maxWidth: "900px",
    width: "100%",
  },
  alertRuleData: {
    fontSize: "16px",
    fontWeight: 600,
  },
  cardBodyText: {
    fontSize: 16,
    overflowWrap: "break-word",
  },
  healthRiskCode: {
    fontSize: 16,
    fontWeight: 700,
  },
  textInputLabel: {
    top: -10,
  },
  content: {
    margin: "20px -8px",
  },
  cardInfoTextTitle: {
    marginBottom: "4px",
  },
  cardInfoText: {
    fontWeight: 400,
    color: "#a0a0a0",
  },
}));

export const HealthRiskCardEditable = ({
  healthRisk,
  setHealthRisk,
  disabled = false,
  rtl = false,
  ...cardProps
}) => {
  const [editing, setEditing] = useState(false);

  const classes = useStyle();

  return (
    <Card className={classes.card} {...cardProps}>
      <CardContent>
        {editing ? (
          <HealthRiskCardForm
            healthRisk={healthRisk}
            setHealthRisk={setHealthRisk}
            setEditing={setEditing}
            rtl={rtl}
          />
        ) : (
          <HealthRiskCardView
            healthRisk={healthRisk}
            setEditing={setEditing}
            rtl={rtl}
            disabled={disabled}
          />
        )}
      </CardContent>
    </Card>
  );
};

// Title for the healthRisk card
const HealthRiskCardTitle = ({ healthRisk, rtl }) => {
  return (
    <Grid container wrap="nowrap">
      {rtl ? (
        <>
          <Typography
            variant="h3"
            style={{
              marginRight: "15px",
              overflowWrap: "anywhere",
              fontWeight: 400,
            }}
          >
            {healthRisk.healthRiskName}
          </Typography>
          <Typography variant="h3">{healthRisk.healthRiskCode}</Typography>
        </>
      ) : (
        <>
          <Typography variant="h3">{healthRisk.healthRiskCode}</Typography>
          <Typography
            variant="h3"
            style={{
              marginLeft: "15px",
              overflowWrap: "anywhere",
              fontWeight: 400,
            }}
          >
            {healthRisk.healthRiskName}
          </Typography>
        </>
      )}
    </Grid>
  );
};

// Component for displaying a section containing healthRisk information
const HealthRiskCardView = ({ healthRisk, setEditing, disabled, rtl }) => {
  const classes = useStyle();

  return (
    <>
      {/* Header */}
      <Grid
        container
        direction="row"
        justifyContent="space-between"
        wrap="nowrap"
      >
        <Grid item style={{ width: "100%" }}>
          <HealthRiskCardTitle healthRisk={healthRisk} rtl={rtl} />
        </Grid>
        {!disabled && (
          <Grid item>
            <Grid
              container
              direction="row"
              justifyContent="flex-end"
              wrap="nowrap"
              style={{ width: "130px" }}
            >
              <Button
                startIcon={<EditIcon className={`${rtl && classes.icon}`} />}
                className={classes.button}
                variant="outlined"
                color="primary"
                onClick={() => setEditing(true)}
              >
                {strings(stringKeys.common.buttons.edit)}
              </Button>
            </Grid>
          </Grid>
        )}
      </Grid>
      {/* Content */}
      <Grid style={{ marginTop: "20px" }}>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Typography variant={"h5"} className={classes.cardInfoTextTitle}>
              {strings(stringKeys.project.form.caseDefinition)}
            </Typography>
            <Typography
              variant={"body1"}
              className={classes.cardBodyText}
              gutterBottom
            >
              {healthRisk.caseDefinition}
            </Typography>
          </Grid>

          <Grid item xs={12} sm={6}>
            <Typography variant={"h5"} className={classes.cardInfoTextTitle}>
              {strings(stringKeys.project.form.feedbackMessage)}
            </Typography>
            <Typography
              variant={"body1"}
              className={classes.cardBodyText}
              gutterBottom
            >
              {healthRisk.feedbackMessage}
            </Typography>
          </Grid>
        </Grid>

        <Typography
          variant={"h5"}
          style={{ marginBottom: "4px", marginTop: "30px" }}
        >
          {strings(stringKeys.project.form.alertsSection)}
        </Typography>

        {healthRisk.healthRiskType === "Activity" ? (
          <Typography
            variant="body1"
            className={`${classes.cardBodyText} ${classes.cardInfoText}`}
          >
            {strings(stringKeys.healthRisk.form.noAlertRule)}
          </Typography>
        ) : (
          <Fragment>
            {(!healthRisk.alertRuleCountThreshold ||
              healthRisk.alertRuleCountThreshold === 0) && (
              <Typography
                variant="body1"
                className={`${classes.cardBodyText} ${classes.cardInfoText}`}
              >
                {strings(stringKeys.common.boolean.false)}
              </Typography>
            )}

            {/* Alert rules */}
            {healthRisk.alertRuleCountThreshold > 0 && (
              <Grid
                container
                justifyContent="flex-start"
                style={{ gap: "40px" }}
              >
                <Typography className={classes.cardBodyText}>
                  {strings(stringKeys.project.form.alertRuleCountThreshold)}:{" "}
                  <b>{healthRisk.alertRuleCountThreshold}</b>
                </Typography>

                {healthRisk.alertRuleCountThreshold > 1 && (
                  <>
                    <Typography className={classes.cardBodyText}>
                      {strings(stringKeys.project.form.alertRuleDaysThreshold)}:{" "}
                      <b>
                        {healthRisk.alertRuleDaysThreshold}{" "}
                        {healthRisk.alertRuleDaysThreshold === 1
                          ? strings(stringKeys.project.form.alertRuleDay)
                          : strings(stringKeys.project.form.alertRuleDays)}{" "}
                      </b>
                    </Typography>

                    <Typography className={classes.cardBodyText}>
                      {strings(
                        stringKeys.project.form.alertRuleKilometersThreshold,
                      )}
                      :{" "}
                      <b>
                        {" "}
                        {healthRisk.alertRuleKilometersThreshold}{" "}
                        {strings(stringKeys.project.form.alertRuleKilometer)}{" "}
                      </b>
                    </Typography>
                  </>
                )}
              </Grid>
            )}
          </Fragment>
        )}
      </Grid>
    </>
  );
};

// Component for displaying a section with an editable healthRisk form
const HealthRiskCardForm = ({
  healthRisk,
  setHealthRisk,
  setEditing,
  rtl = false,
}) => {
  const classes = useStyle();

  const [alertThreshold, setAlertThreshold] = useState(
    healthRisk.alertRuleCountThreshold,
  );

  const form = useMemo(() => {
    const fields = {
      caseDefinition: healthRisk.caseDefinition || "",
      feedbackMessage: healthRisk.feedbackMessage || "",
      alertRuleCountThreshold: healthRisk.alertRuleCountThreshold || undefined,
      alertRuleDaysThreshold: healthRisk.alertRuleDaysThreshold || undefined,
      alertRuleKilometersThreshold:
        healthRisk.alertRuleKilometersThreshold || undefined,
    };

    const validation = {
      caseDefinition: [validators.required, validators.maxLength(500)],
      feedbackMessage: [validators.required, validators.maxLength(160)],
      alertRuleCountThreshold: [
        validators.nonNegativeNumber,
        validators.inRange(0, 99),
      ],
      alertRuleDaysThreshold: [
        validators.requiredWhen((form) => form.alertRuleCountThreshold > 1),
        validators.inRange(1, 365),
      ],
      alertRuleKilometersThreshold: [
        validators.requiredWhen((form) => form.alertRuleCountThreshold > 1),
        validators.inRange(1, 9999),
      ],
    };

    const refs = {
      caseDefinition: createRef(),
      feedbackMessage: createRef(),
      alertRuleCountThreshold: createRef(),
      alertRuleDaysThreshold: createRef(),
      alertRuleKilometersThreshold: createRef(),
    };

    return createForm(fields, validation, refs);
  }, []);

  //Subscribe to changes in alertRuleCountThreshold to show/remove days and kilometers thresholds fields
  form.fields.alertRuleCountThreshold.subscribe((update) => {
    setAlertThreshold(update.newValue);
    if (Number(update.newValue) <= 1) {
      form.fields.alertRuleDaysThreshold.update(undefined);
      form.fields.alertRuleKilometersThreshold.update(undefined);
    }
  });

  const handleFormSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    // setHealthRisk function should set a new value for the given healthRisk, so we must include fields such as healthRiskId
    // from the original healthRisk in order to not lose important values when submitting the form
    setHealthRisk({
      ...healthRisk,
      caseDefinition: form.fields.caseDefinition.value,
      feedbackMessage: form.fields.feedbackMessage.value,
      alertRuleCountThreshold: Number(
        form.fields.alertRuleCountThreshold.value,
      ),
      alertRuleDaysThreshold: Number(form.fields.alertRuleDaysThreshold.value),
      alertRuleKilometersThreshold: Number(
        form.fields.alertRuleKilometersThreshold.value,
      ),
    });
    setEditing(false);
  };

  return (
    <Form onSubmit={handleFormSubmit} fullWidth style={{ margin: "0px" }}>
      {/* Header */}
      <Grid
        container
        direction="row"
        justifyContent="space-between"
        wrap="nowrap"
      >
        <HealthRiskCardTitle healthRisk={healthRisk} rtl={rtl} />
        <Grid item>
          <Grid
            container
            direction="row"
            justifyContent="space-between"
            wrap="nowrap"
            style={{ width: "130px" }}
          >
            <Grid item>
              <Button
                className={classes.button}
                variant="text"
                color="primary"
                onClick={() => setEditing(false)}
              >
                {strings(stringKeys.form.cancel)}
              </Button>
            </Grid>
            <Grid item>
              <Button
                className={classes.button}
                type="submit"
                variant="contained"
                color="primary"
              >
                {strings(stringKeys.common.buttons.update)}
              </Button>
            </Grid>
          </Grid>
        </Grid>
      </Grid>

      {/* Content */}
      <Grid>
        <Grid container spacing={2} className={classes.content}>
          <Grid item xs={12} sm={6}>
            <TextInputField
              label={
                <Typography variant="h5" style={{ bottom: "10px" }}>
                  {strings(stringKeys.project.form.caseDefinition)}
                </Typography>
              }
              classNameLabel={classes.textInputLabel}
              name={`healthRisk.${healthRisk.healthRiskId}.caseDefinition`}
              field={form.fields.caseDefinition}
              multiline
              rows={4}
              fieldRef={form.fields.caseDefinition.ref}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextInputField
              label={
                <Typography variant="h5">
                  {strings(stringKeys.project.form.feedbackMessage)}
                </Typography>
              }
              classNameLabel={classes.textInputLabel}
              name={`healthRisk.${healthRisk.healthRiskId}.feedbackMessage`}
              field={form.fields.feedbackMessage}
              multiline
              rows={4}
              fieldRef={form.fields.feedbackMessage.ref}
            />
          </Grid>
        </Grid>

        <Typography variant="h5">
          {strings(stringKeys.project.form.alertsSection)}
        </Typography>

        {healthRisk.healthRiskType === "Activity" ? (
          <Typography variant="body1" className={classes.disabled}>
            {strings(stringKeys.healthRisk.form.noAlertRule)}
          </Typography>
        ) : (
          <Fragment>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={4}>
                <TextInputField
                  label={
                    <Typography className={classes.cardBodyText}>
                      {strings(stringKeys.project.form.alertRuleCountThreshold)}
                    </Typography>
                  }
                  name={`healthRisk.${healthRisk.healthRiskId}.alertRuleCountThreshold`}
                  field={form.fields.alertRuleCountThreshold}
                  inputMode={"numeric"}
                  fieldRef={form.fields.alertRuleCountThreshold.ref}
                />
              </Grid>

              {/* Display DaysThreshold field if CountThreshold has been set to higher than 1 */}
              {!!(alertThreshold && alertThreshold > 1) && (
                <Grid item xs={12} sm={4}>
                  <TextInputField
                    label={
                      <Typography className={classes.cardBodyText}>
                        {strings(
                          stringKeys.project.form.alertRuleDaysThresholdEdit,
                        )}
                      </Typography>
                    }
                    name={`healthRisk.${healthRisk.healthRiskId}.alertRuleDaysThreshold`}
                    field={form.fields.alertRuleDaysThreshold}
                    inputMode={"numeric"}
                    fieldRef={form.fields.alertRuleDaysThreshold.ref}
                  />
                </Grid>
              )}

              {/* Display KilometersThreshold field if CountThreshold has been set to higher than 1 */}
              {!!(alertThreshold && alertThreshold > 1) && (
                <Grid item xs={12} sm={4}>
                  <TextInputField
                    label={
                      <Typography className={classes.cardBodyText}>
                        {strings(
                          stringKeys.project.form
                            .alertRuleKilometersThresholdEdit,
                        )}
                      </Typography>
                    }
                    name={`healthRisk.${healthRisk.healthRiskId}.alertRuleKilometersThreshold`}
                    field={form.fields.alertRuleKilometersThreshold}
                    inputMode={"numeric"}
                    fieldRef={form.fields.alertRuleKilometersThreshold.ref}
                  />
                </Grid>
              )}
            </Grid>
          </Fragment>
        )}
      </Grid>
    </Form>
  );
};
