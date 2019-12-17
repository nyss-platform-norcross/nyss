import styles from "./NationalSocietyStructureTree.module.scss"

import React, { Fragment } from 'react';
import { Loading } from '../common/loading/Loading';
import TreeView from '@material-ui/lab/TreeView';
import ExpandMoreIcon from '@material-ui/icons/ExpandMore';
import ChevronRightIcon from '@material-ui/icons/ChevronRight';
import { StructureTreeItem } from "./NationalSocietyStructureTreeItem";
import { strings, stringKeys } from "../../strings";
import { InlineTextEditor } from "../common/InlineTextEditor/InlineTextEditor";
import Icon from "@material-ui/core/Icon";

const AddPanel = ({ placeholder, onSave }) => {
  return (
    <div className={styles.addPanel}>
      <Icon className={styles.addPanelIcon}>add</Icon>
      <InlineTextEditor
        placeholder={placeholder}
        onSave={onSave}
      />
    </div>
  );
}

export const NationalSocietyStructureTree = (props) => {
  const { regions, isFetching, nationalSocietyId } = props;

  if (isFetching || !regions) {
    return <Loading />;
  }

  const renderRegions = (regions) =>
    <Fragment>
      {regions.map(region => (
        <StructureTreeItem
          key={`region_${region.id}`}
          itemKey="region"
          item={region}
          onRemove={props.removeRegion}
          onEdit={props.editRegion}
        >
          {renderDistricts(region.id)}
        </StructureTreeItem>
      ))}
      <AddPanel
        placeholder={strings(stringKeys.nationalSociety.structure.addRegion, true)}
        onSave={name => props.createRegion(nationalSocietyId, name)}
      />
    </Fragment>

  const renderDistricts = (regionId) =>
    <Fragment>
      {props.districts.filter(d => d.regionId === regionId).map(district => (
        <StructureTreeItem
          key={`district_${district.id}`}
          itemKey="district"
          item={district}
          onRemove={props.removeDistrict}
          onEdit={props.editDistrict}
        >
          {renderVillages(district.id)}
        </StructureTreeItem>
      ))}
      <AddPanel
        placeholder={strings(stringKeys.nationalSociety.structure.addDistrict, true)}
        onSave={name => props.createDistrict(regionId, name)}
      />
    </Fragment>

  const renderVillages = (districtId) =>
    <Fragment>
      {props.villages.filter(d => d.districtId === districtId).map(village => (
        <StructureTreeItem
          key={`village_${village.id}`}
          itemKey="village"
          item={village}
          onRemove={props.removeVillage}
          onEdit={props.editVillage}
        >
          {renderZones(village.id)}
        </StructureTreeItem>
      ))}
      <AddPanel
        placeholder={strings(stringKeys.nationalSociety.structure.addVillage, true)}
        onSave={name => props.createVillage(districtId, name)}
      />
    </Fragment>

  const renderZones = (villageId) =>
    <Fragment>
      {props.zones.filter(d => d.villageId === villageId).map(zone => (
        <StructureTreeItem
          key={`zone_${zone.id}`}
          itemKey="zone"
          item={zone}
          onRemove={props.removeZone}
          onEdit={props.editZone}
        />
      ))}
      <AddPanel
        placeholder={strings(stringKeys.nationalSociety.structure.addZone, true)}
        onSave={name => props.createZone(villageId, name)}
      />
    </Fragment>

  return (
    <TreeView
      defaultCollapseIcon={<ExpandMoreIcon />}
      defaultExpandIcon={<ChevronRightIcon />}
      className={styles.tree}
      expanded={props.expandedItems}
      onNodeToggle={(e, items) => props.updateExpandedItems(items)}
    >
      {renderRegions(regions)}
    </TreeView>
  );
}
