import { useEffect, useState } from "react";
import { MenuItem, Checkbox } from "@material-ui/core";
import MultiSelectField from "../../forms/MultiSelectField";
import { strings, stringKeys } from "../../../strings";
import styles from "./HealthRiskFilter.module.scss";
import { SelectAll } from "../../common/selectAll/SelectAll";

export const HealthRiskFilter = ({
  allHealthRisks,
  filteredHealthRisks,
  onChange,
  updateValue,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  // Checks off all boxes on mount
  useEffect(() => {
    // If a new health risk is created or deleted and the user returns to a dashboard, the filtered health risk is updated
    // It also doesn´t run if filteredHealthRisks is the same as allHealthRisks.
    if (allHealthRisks.length !== filteredHealthRisks.length) {
      onChange(allHealthRisks.map((hr) => hr.id))
    }
  }, [allHealthRisks]);

  // Handles when the checkbox is checked off or not checked on. Will only update filteredHealthRisks to not fetch from backend every time.
  const handleHealthRiskChange = (event) => {
    updateValue({
      healthRisks:
        typeof event.target.value === "string"
          ? event.target.value.split(",")
          : event.target.value,
    });
  };

  // Handles when select all checkbox is toggled on or off. Same functionality as handleHealthRiskChange.
  const toggleSelectAll = () => {
    updateValue({
      healthRisks:
        filteredHealthRisks.length === allHealthRisks.length
          ? []
          : allHealthRisks.map((hr) => hr.id),
    });
  };

  // Displays the text of the dropdown i.e if all are selected, then "All" is displayed or if Acute malnutrition and Fever and rash are selected then "Acute malnutrition (+1)" is displayed.
  const renderHealthRiskValues = (selectedIds) =>
    selectedIds.length < 1 || selectedIds.length === allHealthRisks.length
      ? strings(stringKeys.dashboard.filters.healthRiskAll)
      : selectedIds.map(
        (id) => allHealthRisks?.find((hr) => hr.id === id)?.name
      )[0] +
      `${selectedIds.length > 1 ? ` (+${selectedIds.length - 1})` : ""}`;

  // Uses the onChange function to fetch from backend
  const showResults = () => {
    onChange(filteredHealthRisks);
    setIsOpen(false);
  };

  return (
    <MultiSelectField
      name="healthRisks"
      label={strings(stringKeys.dashboard.filters.healthRisk)}
      onChange={handleHealthRiskChange}
      value={filteredHealthRisks}
      renderValues={renderHealthRiskValues}
      className={styles.healthRiskFilter}
      isOpen={isOpen}
      setIsOpen={setIsOpen}
      showResults={showResults}
    >
      <SelectAll
        isSelectAllEnabled={
          filteredHealthRisks.length === allHealthRisks.length
        }
        toggleSelectAll={toggleSelectAll}
        showResults={showResults}
      />
      {allHealthRisks.map((hr) => (
        <MenuItem
          key={`filter_healthRisk_${hr.id}`}
          value={hr.id}
          className={styles.healtRiskMenuItem}
        >
          <Checkbox
            color="primary"
            checked={filteredHealthRisks.indexOf(hr.id) > -1}
          />
          <span style={{ width: "90%", whiteSpace: 'pre-wrap', wordWrap: "break-word", overflowWrap: 'break-word' }}>
            {hr.name}
          </span>
        </MenuItem>
      ))
      }
    </MultiSelectField >
  );
};
