import { useState } from "react";

const useLocalFilters = (filters) => {
  const [filter, setFilter] = useState(filters);

  const updateFilter = (change) => {
    const newValue = {
      ...filter,
      ...change,
    };
    setFilter((prev) => ({ ...prev, ...change }));
    return newValue;
  };

  return [filter, updateFilter];
};

export default useLocalFilters;
