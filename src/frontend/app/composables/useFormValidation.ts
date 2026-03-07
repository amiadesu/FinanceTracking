import { ref, computed, watch } from 'vue';
import * as v from 'valibot';

export function useFormValidation(schema: any, state: Record<string, any>) {
    const isTouched = ref(false);
    
    const touch = () => { isTouched.value = true; };

    const validationResult = computed(() => v.safeParse(schema, state));
    const isFormValid = computed(() => validationResult.value.success);

    watch(isFormValid, (isValid) => {
        if (isValid) {
            isTouched.value = false;
        }
    });
    
    const unmappedErrors = computed(() => {
        if (!isTouched.value || validationResult.value.success) return [];
        
        const issues = validationResult.value.issues;

        const hasMappedErrors = issues.some((issue) => issue.path && issue.path.length > 0);
        if (hasMappedErrors) return [];
        
        return issues
            .filter((issue) => !issue.path || issue.path.length === 0)
            .map((issue) => issue.message);
    });

    return {
        isFormValid,
        unmappedErrors,
        touch
    };
}