import React, { useEffect, useState } from "react";
import { connect } from "react-redux";
import { Grid, Typography, Card, CardContent } from "@material-ui/core";
import Layout from "../layout/Layout";
import { withLayout } from "../../utils/layout";

const MessageTitles = {
  "report.errorType.healthRiskNotFound": "Health risk not used in project",
  "report.errorType.dataCollectorUsedCollectionPointFormat":
    "Wrong reporting format: Data collector used data collection point reporting format",
  "report.errorType.collectionPointUsedDataCollectorFormat":
    "Wrong reporting format: Data collection point used data collector reporting format",
  "report.errorType.gateway": "SMS gateway error - contact manager",
};

const ProjectErrorMessagesPageComponent = (props) => {
  const [errorMessages, setErrorMessages] = useState([]);

  async function fetchData() {
    const url = `/api/project/${props.projectId}/errorMessages`;
    const response = await fetch(url);

    if (!response.ok) return;

    setErrorMessages(await response.json());
  }

  useEffect(() => {
    fetchData();
  }, []);

  return (
    <Grid container spacing={4} fixed="true" style={{ maxWidth: 800 }}>
      {errorMessages.map((itm) => (
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h3">{MessageTitles[itm.key]}</Typography>
              <Typography variant="body1" gutterBottom>
                {itm.message}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
};

const mapStateToProps = (_, ownProps) => ({
  projectId: ownProps.match.params.projectId,
});

const mapDispatchToProps = {};

export const ProjectErrorMessagesPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps
  )(ProjectErrorMessagesPageComponent)
);
