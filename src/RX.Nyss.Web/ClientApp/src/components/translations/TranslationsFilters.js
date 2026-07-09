import styles from "./TranslationsFilters.module.scss";
import React, { useState } from "react";
import {
  Card,
  CardContent,
  Grid,
  FormControlLabel,
  Checkbox,
} from "@material-ui/core";

export const TranslationsFilters = ({ onChange }) => {
  const [needsImprovementOnly, setNeedsImprovementOnly] = useState(false);

  const handleChange = (e) => {
    setNeedsImprovementOnly(e.target.checked);
    onChange(e.target.checked);
  };

  return (
    <Card className={styles.filters}>
      <CardContent>
        <Grid container spacing={2}>
          <FormControlLabel
            control={
              <Checkbox
                checked={needsImprovementOnly}
                onChange={handleChange}
              />
            }
            name={"needsImprovementOnly"}
            label={"Only strings that need improvement"}
          />
        </Grid>
      </CardContent>
    </Card>
  );
};
