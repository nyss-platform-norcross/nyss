export const ReportListType = {
    unknownSender: 'unknownSender',
    main: 'main',
    fromDcp: 'fromDcp'
}

export const ReportErrorFilterType = {
    all: 'All',
    healthRiskNotFound: 'HealthRiskNotFound',
    wrongFormat: 'WrongFormat',
    gatewayError: 'GatewayError',
    other: 'Other'
}

export const reportErrorFilterTypes = [
    ReportErrorFilterType.all,
    ReportErrorFilterType.healthRiskNotFound,
    ReportErrorFilterType.wrongFormat,
    ReportErrorFilterType.gatewayError,
    ReportErrorFilterType.other
]