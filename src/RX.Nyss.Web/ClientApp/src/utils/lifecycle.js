import { useEffect } from "react";

export const useMount = (mount) => useEffect(mount, []);
