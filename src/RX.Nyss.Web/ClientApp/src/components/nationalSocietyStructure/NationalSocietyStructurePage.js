import React, { useEffect, Fragment } from 'react';
import { connect } from "react-redux";
import * as nationalSocietyStructureActions from './logic/nationalSocietyStructureActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import Typography from '@material-ui/core/Typography';
import { strings, stringKeys } from '../../strings';
import { NationalSocietyStructureTree } from './NationalSocietyStructureTree';

const NationalSocietyStructurePageComponent = (props) => {
  const { regions, districts, villages, zones, isFetching, openStructure, nationalSocietyId } = props;

  useEffect(() => {
    openStructure(nationalSocietyId);
  }, [openStructure, nationalSocietyId])

  if (!regions) {
    return null;
  }

  return (
    <Fragment>
      <Typography variant="body1">
        {strings(stringKeys.nationalSociety.structure.introduction)}
      </Typography>

      <NationalSocietyStructureTree
        regions={regions}
        districts={districts}
        villages={villages}
        zones={zones}
        isFetching={isFetching}
        nationalSocietyId={nationalSocietyId}
        nationalSocietyIsArchived = {props.nationalSocietyIsArchived}

        expandedItems={props.expandedItems}
        updateExpandedItems={props.updateExpandedItems}

        createRegion={props.createRegion}
        editRegion={props.editRegion}
        removeRegion={props.removeRegion}

        createDistrict={props.createDistrict}
        editDistrict={props.editDistrict}
        removeDistrict={props.removeDistrict}

        createVillage={props.createVillage}
        editVillage={props.editVillage}
        removeVillage={props.removeVillage}

        createZone={props.createZone}
        editZone={props.editZone}
        removeZone={props.removeZone}
      />
    </Fragment>
  );
}

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.nationalSocietyStructure.structureFetching,
  regions: state.nationalSocietyStructure.regions,
  districts: state.nationalSocietyStructure.districts,
  villages: state.nationalSocietyStructure.villages,
  zones: state.nationalSocietyStructure.zones,
  expandedItems: state.nationalSocietyStructure.expandedItems,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived
});

const mapDispatchToProps = {
  openStructure: nationalSocietyStructureActions.openStructure.invoke,

  updateExpandedItems: nationalSocietyStructureActions.updateExpandedItems,

  createRegion: nationalSocietyStructureActions.createRegion.invoke,
  editRegion: nationalSocietyStructureActions.editRegion.invoke,
  removeRegion: nationalSocietyStructureActions.removeRegion.invoke,

  createDistrict: nationalSocietyStructureActions.createDistrict.invoke,
  editDistrict: nationalSocietyStructureActions.editDistrict.invoke,
  removeDistrict: nationalSocietyStructureActions.removeDistrict.invoke,

  createVillage: nationalSocietyStructureActions.createVillage.invoke,
  editVillage: nationalSocietyStructureActions.editVillage.invoke,
  removeVillage: nationalSocietyStructureActions.removeVillage.invoke,

  createZone: nationalSocietyStructureActions.createZone.invoke,
  editZone: nationalSocietyStructureActions.editZone.invoke,
  removeZone: nationalSocietyStructureActions.removeZone.invoke
};

export const NationalSocietyStructurePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyStructurePageComponent)
);
