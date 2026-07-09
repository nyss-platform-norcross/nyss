import styles from "./DatePicker.module.scss";
import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import {
  KeyboardDatePicker,
  MuiPickersUtilsProvider,
} from "@material-ui/pickers";
import format from "date-fns/format";
import frLocale from "date-fns/locale/fr";
import arLocale from "date-fns/locale/ar";
import esLocale from "date-fns/locale/es";
import enLocale from "date-fns/locale/en-GB";
import DateFnsUtils from "@date-io/date-fns";
import { useSelector } from "react-redux";

class LocalizedUtils extends DateFnsUtils {
  getDatePickerHeaderText(date) {
    return format(date, "yyyy-MM-dd", { locale: this.locale });
  }
}

export const DatePicker = ({
  label,
  value,
  onChange,
  className,
  fullWidth,
}) => {
  const userLanguageCode = useSelector(
    (state) => state.appData.user.languageCode,
  );
  const [locale, setLocale] = useState(enLocale);

  useEffect(() => {
    switch (userLanguageCode) {
      case "en":
        setLocale(enLocale);
        break;
      case "es":
        setLocale(esLocale);
        break;
      case "fr":
        setLocale(frLocale);
        break;
      case "ar":
        setLocale(arLocale);
        break;
      default:
        setLocale(enLocale);
    }
  }, [userLanguageCode]);

  return (
    <MuiPickersUtilsProvider utils={LocalizedUtils} locale={locale}>
      <KeyboardDatePicker
        className={`${className} ${fullWidth ? "" : styles.datePicker}`}
        autoOk
        disableFuture
        disableToolbar
        variant="inline"
        format="yyyy-MM-dd"
        onChange={onChange}
        label={label}
        value={value}
        InputProps={{ readOnly: true }}
        InputLabelProps={{ shrink: true }}
      />
    </MuiPickersUtilsProvider>
  );
};

DatePicker.propTypes = {
  label: PropTypes.string,
  value: PropTypes.any,
  onChange: PropTypes.func,
  className: PropTypes.string,
  fullWidth: PropTypes.bool,
};
