import styles from "./NationalSocietyStructureTree.module.scss"

import React, { Fragment } from 'react';
import { Loading } from '../common/loading/Loading';
import TreeView from '@material-ui/lab/TreeView';
import ExpandMoreIcon from '@material-ui/icons/ExpandMore';
import ChevronRightIcon from '@material-ui/icons/ChevronRight';
import { StructureTreeItem } from "./NationalSocietyStructureTreeItem";
import { strings, stringKeys } from "../../strings";
import { InlineTextEditor } from "../common/InlineTextEditor/InlineTextEditor";
import { Icon } from "@material-ui/core";
import * as roles from "../../authentication/roles";

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

  const canModify =
    !props.nationalSocietyIsArchived
    && (
      !props.nationalSocietyHasCoordinator
      || props.callingUserRoles.some(r => r === roles.Coordinator || r === roles.Administrator)
    );

  const renderRegions = (regions) => {
    if (!regions.length && !canModify) {
      return null;
    }

    return (
      <Fragment>
        {regions.map(region => (
          <StructureTreeItem
            key={`region_${region.id}`}
            itemKey="region"
            item={region}
            onRemove={props.removeRegion}
            onEdit={props.editRegion}
            canModify={canModify}
          >
            {renderDistricts(region.id)}
          </StructureTreeItem>
        ))}
        {canModify && (
          <AddPanel
            placeholder={strings(stringKeys.nationalSociety.structure.addRegion, true)}
            onSave={name => props.createRegion(nationalSocietyId, name)}
          />
        )}
      </Fragment>
    );
  }

  const renderDistricts = (regionId) => {
    const list = props.districts.filter(d => d.regionId === regionId);

    if (!list.length && !canModify) {
      return null;
    }

    return (
      <Fragment>
        {list.map(district => (
          <StructureTreeItem
            key={`district_${district.id}`}
            itemKey="district"
            item={district}
            onRemove={props.removeDistrict}
            onEdit={props.editDistrict}
            canModify={canModify}
          >
            {renderVillages(district.id)}
          </StructureTreeItem>
        ))}
        {canModify && (
          <AddPanel
            placeholder={strings(stringKeys.nationalSociety.structure.addDistrict, true)}
            onSave={name => props.createDistrict(regionId, name)}
          />
        )}
      </Fragment>
    );
  }

  const renderVillages = (districtId) => {
    const list = props.villages.filter(d => d.districtId === districtId);

    if (!list.length && !canModify) {
      return null;
    }

    return (
      <Fragment>
        {list.map(village => (
          <StructureTreeItem
            key={`village_${village.id}`}
            itemKey="village"
            item={village}
            onRemove={props.removeVillage}
            onEdit={props.editVillage}
            canModify={canModify}
          >
            {renderZones(village.id)}
          </StructureTreeItem>
        ))}
        {canModify && (
          <AddPanel
            placeholder={strings(stringKeys.nationalSociety.structure.addVillage, true)}
            onSave={name => props.createVillage(districtId, name)}
          />
        )}
      </Fragment>
    );
  }

  const renderZones = (villageId) => {
    const list = props.zones.filter(d => d.villageId === villageId);

    if (!list.length && !canModify) {
      return null;
    }

    return (
      <Fragment>
        {list.map(zone => (
          <StructureTreeItem
            key={`zone_${zone.id}`}
            itemKey="zone"
            item={zone}
            onRemove={props.removeZone}
            onEdit={props.editZone}
            canModify={canModify}
          />
        ))}
        {canModify && (
          <AddPanel
            placeholder={strings(stringKeys.nationalSociety.structure.addZone, true)}
            onSave={name => props.createZone(villageId, name)}
          />
        )}
      </Fragment>
    );
  }

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
