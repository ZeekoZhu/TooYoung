// Workaround for Office Fabric SSR
import { configureLoadStyles } from '@microsoft/load-themed-styles';

// Store registered styles in a variable used later for injection.
let allStyles: string[] = [];

// Push styles into variables for injecting later.
configureLoadStyles((styles: string) => {
    allStyles.push(styles);
});

export const styles = allStyles;
