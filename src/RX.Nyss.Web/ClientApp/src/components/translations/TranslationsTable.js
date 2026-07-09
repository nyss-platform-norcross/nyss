import React, { useState } from "react";
import PropTypes from "prop-types";
import { Loading } from "../common/loading/Loading";
import { strings, stringKeys } from "../../strings";
import { StringsEditor } from "../common/stringsEditor/StringsEditor";
import {
  TableSortLabel,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableContainer
} from "@material-ui/core";

export const TranslationsTable = ({
  isListFetching,
  languages,
  translations,
  type,
}) => {
  const [sorting, setSorting] = useState({ orderBy: "key" });

  if (isListFetching) {
    return <Loading />;
  }

  const createSortHandler = (column) => (event) => {
    handleSortChange(event, column);
  };

  const handleSortChange = (event, column) => {
    const isAscending = sorting.orderBy === column && sorting.sortAscending;
    setSorting({ orderBy: column, sortAscending: !isAscending });
  };

  const sortedTranslations = () => {
    const sorted = [...translations].sort((a, b) =>
      sorting.orderBy === "key"
        ? a.key > b.key
          ? 1
          : -1
        : a.translations[sorting.orderBy] > b.translations[sorting.orderBy]
          ? 1
          : -1,
    );

    return sorting.sortAscending ? sorted : [...sorted].reverse();
  };

  return (
    <TableContainer>
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell>
              <TableSortLabel
                active={sorting.orderBy === "key"}
                direction={sorting.sortAscending ? "asc" : "desc"}
                onClick={createSortHandler("key")}
              >
                {strings(stringKeys.translations.list.key)}
              </TableSortLabel>
            </TableCell>
            {languages.map((language) => (
              <TableCell key={language.languageCode}>
                <TableSortLabel
                  active={sorting.orderBy === language.languageCode}
                  direction={sorting.sortAscending ? "asc" : "desc"}
                  onClick={createSortHandler(language.languageCode)}
                >
                  {language.displayName}
                </TableSortLabel>
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {sortedTranslations().map((translation) => (
            <TableRow key={translation.key}>
              <TableCell>
                <StringsEditor
                  stringKey={translation.key}
                  type={type}
                  needsImprovement={translation.needsImprovement}
                />
              </TableCell>
              {languages.map((language) => (
                <TableCell
                  key={`translation_${translation.key}_${language.languageCode}`}
                >
                  {translation.translations[language.languageCode]}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

TranslationsTable.propTypes = {
  isFetching: PropTypes.bool,
  languages: PropTypes.array,
  translation: PropTypes.array,
  type: PropTypes.string,
};

export default TranslationsTable;
