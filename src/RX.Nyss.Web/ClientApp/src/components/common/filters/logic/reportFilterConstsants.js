export const DataCollectorType = {
    unknownSender: 'unknownSender',
    human: 'human',
    collectionPoint: 'collectionPoint'
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