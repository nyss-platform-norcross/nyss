import React, { useState } from "react";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import { Loading } from "../common/loading/Loading";
import * as projectSetupActions from "./logic/projectSetupActions";
import * as projectActions from "../projects/logic/projectsActions";
import { useMount } from "../../utils/lifecycle";
import { SetupStepper } from "../common/stepper/SetupStepper";
import { ProjectSetupName } from "./ProjectSetupName";
import { ProjectSetupOrganization } from "./ProjectSetupOrganization";
import { ProjectSetupRecipients } from "./ProjectSetupRecipients";
import { ProjectSetupHealthRisk } from "./ProjectSetupHealthRisk";
import { ProjectSetupGeographicalStructure } from "./ProjectSetupGeographicalStructure";
import { ProjectSetupSummary } from "./ProjectSetupSummary";
import { strings, stringKeys } from "../../strings";
import { trackPageView } from "../../utils/appInsightsHelper";

const ProjectSetupPageComponent = ({
  nationalSocietyId,
  isFetching,
  formData,
  openProjectSetup,
  goToList,
  ...props
}) => {
  useMount(() => {
    openProjectSetup(nationalSocietyId);

    // Track page view
    trackPageView("ProjectSetupPage");
  });

  const [error, setError] = useState(false);
  const [isNextStepInvalid, setIsNextStepInvalid] = useState(true);

  if (isFetching || !formData) {
    return <Loading />;
  }

  const projectSetupSteps = [
    {
      name: strings(stringKeys.projectSetup.projectName.name),
      content: (
        <ProjectSetupName
          error={error}
          setError={setError}
          setIsNextStepInvalid={setIsNextStepInvalid}
        />
      ),
      stepNumber: 0,
    },
    {
      name: strings(stringKeys.projectSetup.projectOrganization.name),
      content: (
        <ProjectSetupOrganization
          error={error}
          setError={setError}
          setIsNextStepInvalid={setIsNextStepInvalid}
        />
      ),
      stepNumber: 1,
    },
    {
      name: strings(stringKeys.projectSetup.projectRecipients.name),
      content: (
        <ProjectSetupRecipients
          error={error}
          setError={setError}
          setIsNextStepInvalid={setIsNextStepInvalid}
        />
      ),
      stepNumber: 2,
    },
    {
      name: strings(stringKeys.projectSetup.projectHealthRisks.name),
      content: (
        <ProjectSetupHealthRisk
          error={error}
          setError={setError}
          setIsNextStepInvalid={setIsNextStepInvalid}
        />
      ),
      stepNumber: 3,
    },
    {
      name: strings(stringKeys.projectSetup.geographicalStructure.name),
      content: <ProjectSetupGeographicalStructure />,
      stepNumber: 4,
      isOptional: true,
    },
    {
      name: strings(stringKeys.projectSetup.summary.name),
      content: <ProjectSetupSummary />,
      stepNumber: 5,
    },
  ];

  return (
    <div>
      <SetupStepper
        steps={projectSetupSteps}
        error={error}
        setError={setError}
        isNextStepInvalid={isNextStepInvalid}
        setIsNextStepInvalid={setIsNextStepInvalid}
        goToList={goToList}
        nationalSocietyId={nationalSocietyId}
      />
    </div>
  );
};

ProjectSetupPageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.projectSetup.formFetching,
  formData: state.projectSetup.formData,
});

const mapDispatchToProps = {
  openProjectSetup: projectSetupActions.openSetup.invoke,
  goToList: projectActions.goToList,
};

export const ProjectSetupPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectSetupPageComponent),
);
