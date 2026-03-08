import { useToast } from '#imports';

export const useAppToast = () => {
    const toast = useToast();

    const showSuccess = (description: string, title = 'Success') => {
        toast.add({ title, description, color: 'success' });
    };

    const showError = (description: string, title = 'Error') => {
        toast.add({ title, description, color: 'error' });
    };

    const withConfirm = (options: {
        title: string;
        description: string;
        onConfirm: () => Promise<void> | void;
        confirmLabel?: string;
        toastColor?: "error" | "primary" | "secondary" | "success" | "info" | "warning" | "neutral" | undefined;
        actionColor?: "error" | "primary" | "secondary" | "success" | "info" | "warning" | "neutral" | undefined;
        successMsg?: string;
        errorMsg?: string;
    }) => {
        toast.add({
            title: options.title,
            description: options.description,
            color: options.toastColor || 'warning',
            duration: 0,
            actions: [
                {
                    label: options.confirmLabel || 'Confirm',
                    color: options.actionColor || 'primary',
                    onClick: async () => {
                        try {
                            await options.onConfirm();
                            if (options.successMsg) {
                                showSuccess(options.successMsg);
                            }
                        } catch (err: any) {
                            if (options.errorMsg) {
                                showError(options.errorMsg);
                            } else {
                                showError(err.message || 'An unexpected error occurred.');
                            }
                        }
                    }
                },
                {
                    label: 'Cancel',
                    color: 'neutral',
                    variant: 'outline'
                }
            ]
        });
    };

    return {
        showSuccess,
        showError,
        withConfirm
    };
};