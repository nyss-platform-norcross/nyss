import styles from "./AreaFilter.module.scss";

import React, { Fragment, useRef, useState } from 'react';
import { get } from '../../../utils/http';
import TreeView from '@material-ui/lab/TreeView';
import TreeItem from '@material-ui/lab/TreeItem';
import ExpandMoreIcon from '@material-ui/icons/ExpandMore';
import ArrowDropDown from '@material-ui/icons/ArrowDropDown';
import ChevronRightIcon from '@material-ui/icons/ChevronRight';
import ErrorOutlineIcon from '@material-ui/icons/ErrorOutline';
import {TextField, Menu, InputAdornment} from "@material-ui/core";
import { strings, stringKeys } from "../../../strings";
import {IconTreeItem} from "../icon/IconTreeItem";

const StructureTreeItem = ({ data, onSelect, isSelected, children }) => {
  const handleChange = (e) => {
    e.stopPropagation();
    onSelect(data);
  }

  const treeItemContent = () => (
    <div onClick={handleChange}>
      {data.name}
    </div>
  );

  return (
    <TreeItem
      nodeId={`${data.type}_${data.id}`}
      className={isSelected ? styles.selected : null}
      label={treeItemContent()}>
      {children}
    </TreeItem>
  );
}

export const AreaFilter = ({ nationalSocietyId, selectedItem, onChange, showUnknown=false }) => {
  const [dropdownVisible, setDropdownVisible] = useState(false);
  const [, setIsFetching] = useState(false);
  const [structureLoaded, setStructureLoaded] = useState(false);
  const [regions, setRegions] = useState([]);
  const triggerRef = useRef(null);
  const [expanded, setExpanded] = useState([]);

  const handleDropdownClick = (e) => {
    setDropdownVisible(true);

    if (!structureLoaded) {
      setIsFetching(true);
      get(`/api/nationalSocietyStructure/get?nationalSocietyId=${nationalSocietyId}`)
        .then(response => {
          setIsFetching(false);
          setStructureLoaded(true);
          setRegions(response.value.regions)
        })
        .catch(() => {
          setIsFetching(false);
        })
    }
  }

  const handleDropdownClose = (e) => {
    setDropdownVisible(false);
  };

  const handleChange = (item) => {
    onChange(item);
    setDropdownVisible(false);
  }

  const handleNodeToggle = (event, nodeIds) =>
    setExpanded(nodeIds);

  const selectedItemKey = selectedItem ? `${selectedItem.type}_${selectedItem.id}` : null;

  if (!regions) {
    return null;
  }

  const handleChangeToEmpty = (e) => {
    e.stopPropagation();
    handleChange(null);
  }

  return (
    <Fragment>
      <TextField
        onClick={handleDropdownClick}
        ref={triggerRef}
        className={styles.field}
        label={strings(stringKeys.filters.area.title)}
        InputProps={{
          readOnly: true,
          endAdornment: (
            <InputAdornment >
              <ArrowDropDown className={styles.arrow} />
            </InputAdornment>
          )
        }}
        inputProps={{
          className: styles.areaSelect
        }}
        value={selectedItem ? selectedItem.name : strings(stringKeys.filters.area.all)}
      />
      <Menu
        anchorEl={triggerRef.current}
        onClose={handleDropdownClose}
        open={dropdownVisible}
        className={styles.dropDown}
        PaperProps={{ className: styles.dropDown }}
      >
        <TreeView
          className={styles.tree}
          defaultCollapseIcon={<ExpandMoreIcon />}
          defaultExpandIcon={<ChevronRightIcon />}
          expanded={expanded}
          onNodeToggle={handleNodeToggle}
        >
          <Fragment>
            <TreeItem
              className={!selectedItemKey ? styles.selected : null}
              nodeId='none'
              onClick={handleChangeToEmpty}
              label={strings(stringKeys.filters.area.all)}>
            </TreeItem>

            {showUnknown &&
            <IconTreeItem
              nodeId='unknown'
              className={!selectedItemKey ? styles.selected : null}
              onSelect={handleChange}
              labelText={strings(stringKeys.filters.area.unknown)}
              labelIcon={ErrorOutlineIcon}
            />}

            {regions.map(region => ({ key: `region_${region.id}`, data: region })).map(region => (
              <StructureTreeItem
                key={region.key}
                data={{ type: "region", id: region.data.id, name: region.data.name, parents: [] }}
                isSelected={selectedItemKey === region.key}
                onSelect={handleChange}
                label={region.name}>

                {region.data.districts.map(district => ({ key: `district_${district.id}`, data: district })).map(district => (
                  <StructureTreeItem
                    key={district.key}
                    data={{ type: "district", id: district.data.id, name: district.data.name, parents: [region.key] }}
                    isSelected={selectedItemKey === district.key}
                    onSelect={handleChange}
                    label={district.name}>

                    {district.data.villages.map(village => ({ key: `village_${village.id}`, data: village })).map(village => (
                      <StructureTreeItem
                        key={village.key}
                        data={{ type: "village", id: village.data.id, name: village.data.name, parents: [region.key, district.key] }}
                        isSelected={selectedItemKey === village.key}
                        onSelect={handleChange}
                        label={village.name}>

                        {village.data.zones.map(zone => ({ key: `zone_${zone.id}`, data: zone })).map(zone => (
                          <StructureTreeItem
                            key={zone.key}
                            data={{ type: "zone", id: zone.data.id, name: zone.data.name, parents: [region.key, district.key, village.key] }}
                            isSelected={selectedItemKey === zone.key}
                            onSelect={handleChange}
                            label={zone.name} />
                        ))}
                      </StructureTreeItem>
                    ))}
                  </StructureTreeItem>
                ))}
              </StructureTreeItem>
            ))}
          </Fragment>
        </TreeView>
      </Menu>
    </Fragment>
  );
}
