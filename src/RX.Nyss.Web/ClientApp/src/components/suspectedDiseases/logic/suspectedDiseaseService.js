export const getSaveFormModel = (values, contentLanguages) => ({
  suspectedDiseaseCode: parseInt(values.suspectedDiseaseCode),
  languageContent: contentLanguages.map((lang) => ({
    languageId: lang.id,
    name: values[`contentLanguage_${lang.id}_name`],
  })),
});
